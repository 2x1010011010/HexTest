using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.UI.ResultPopup;
using HexaSortTest.CodeBase.Infrastructure.Logging;
using HexaSortTest.CodeBase.Infrastructure.Services.CurrencyService;
using HexaSortTest.CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using HexaSortTest.CodeBase.Infrastructure.Services.SaveAndLoadService;
using HexaSortTest.CodeBase.Infrastructure.StateMachine.States.CustomPayloadStructures;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States
{
  public class GameResultState : IPayloadState<GameResultPayload>
  {
    private readonly GameStateMachine _gameStateMachine;
    private readonly SceneLoader _sceneLoader;
    private readonly IUIFactory _uiFactory;
    private readonly IPersistentProgressService _progressService;
    private readonly ISaveLoadService _saveLoadService;
    private readonly IGameFactory _gameFactory;
    private readonly ICurrencyService _currencyService;

    private GameResultPopup _popup;
    private bool _isVictory;

    public GameResultState(
      GameStateMachine gameStateMachine,
      SceneLoader sceneLoader,
      IUIFactory uiFactory,
      IPersistentProgressService progressService,
      ISaveLoadService saveLoadService,
      IGameFactory gameFactory,
      ICurrencyService currencyService)
    {
      _gameStateMachine = gameStateMachine;
      _sceneLoader = sceneLoader;
      _uiFactory = uiFactory;
      _progressService = progressService;
      _saveLoadService = saveLoadService;
      _gameFactory = gameFactory;
      _currencyService = currencyService;
    }

    public void Enter(GameResultPayload payload)
    {
      _isVictory = payload.IsVictory;
#if UNITY_EDITOR
      GameLogger.Message($"[GameResultState] Result received: victory={_isVictory}, level index {_progressService.PlayerProgress.LevelIndex}.");
#endif

      if (_isVictory)
        CompleteLevel();

      _sceneLoader.Load(Constants.GameResultScene, onLoaded: SpawnPopup);
    }

    public void Exit()
    {
      if (_popup != null)
      {
        _popup.OnContinueClicked -= HandleContinueClicked;
        _popup.OnMainMenuClicked -= HandleMainMenuClicked;
      }

      _popup = null;
      _uiFactory.Clear();
    }

    // NOTE: reads _gameFactory.CurrentLevelConfig, which is still valid here —
    // GameFactory isn't cleared until the next LoadLevelState.Enter() runs.
    private void CompleteLevel()
    {
      var progress = _progressService.PlayerProgress;
      // A repeated result for the same loaded level must not grant another reward.
      if (_gameFactory.CurrentLevelIndex < 0 || progress.LevelIndex != _gameFactory.CurrentLevelIndex)
        return;

      var levelConfig = _gameFactory.CurrentLevelConfig;
      if (levelConfig == null)
      {
#if UNITY_EDITOR
        GameLogger.Warning("[GameResultState] CurrentLevelConfig is null, skipping reward payout.");
#endif
        return;
      }

      _saveLoadService.ExecuteTransaction(() =>
      {
        progress.LevelIndex++;
        _currencyService.AddCoins(CoinsRewardFor(levelConfig.Difficulty));
        _currencyService.AddHexCoins(levelConfig.WinCondition);
      });
#if UNITY_EDITOR
      GameLogger.Message($"[GameResultState] Rewards granted: {CoinsRewardFor(levelConfig.Difficulty)} Coins, {levelConfig.WinCondition} HexCoins.");
#endif
    }

    private static int CoinsRewardFor(LevelDifficulty difficulty) => difficulty switch
    {
      LevelDifficulty.Easy => Constants.EasyLevelCoinsReward,
      LevelDifficulty.Hard => Constants.HardLevelCoinsReward,
      LevelDifficulty.SuperHard => Constants.SuperHardLevelCoinsReward,
      _ => Constants.EasyLevelCoinsReward
    };

    private void SpawnPopup()
    {
      _popup = _uiFactory.CreateGameResultPopup();

      if (_popup == null)
      {
#if UNITY_EDITOR
        GameLogger.Error("[GameResultState] Failed to spawn GameResultPopup. " +
                        "Check that AssetPaths.GameResultPopupPrefab points to a valid prefab under Resources/.");
#endif
        return;
      }

      _popup.OnContinueClicked += HandleContinueClicked;
      _popup.OnMainMenuClicked += HandleMainMenuClicked;

      if (_isVictory)
        _popup.ShowVictory();
      else
        _popup.ShowDefeat();
    }

    private void HandleContinueClicked()
    {
#if UNITY_EDITOR
      GameLogger.Message($"[GameResultState] Continue requested: victory={_isVictory}.");
#endif
      // Victory was already committed on entry. Defeat keeps the same index.
      ReloadGameScene();
    }

    private void HandleMainMenuClicked() =>
      _gameStateMachine.Enter<MainMenuState>();

    private void ReloadGameScene() =>
      _gameStateMachine.Enter<LoadLevelState, string>(
        _progressService.PlayerProgress.WorldData.LastLevel.Level);
  }
}
