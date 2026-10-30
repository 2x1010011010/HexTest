using System;
using HexaSortTest.CodeBase.Infrastructure.Services.LivesService;
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
    private bool _resultCommitted = true;
    private bool _leaving;
    private int _coinsReward;
    private int _hexCoinsReward;
    private readonly LivesService _lives;

    public void BeginAttempt() => _resultCommitted = false;

    public GameResultState(
      GameStateMachine gameStateMachine,
      SceneLoader sceneLoader,
      IUIFactory uiFactory,
      IPersistentProgressService progressService,
      ISaveLoadService saveLoadService,
      IGameFactory gameFactory,
      ICurrencyService currencyService,
      LivesService lives)
    {
      _gameStateMachine = gameStateMachine;
      _sceneLoader = sceneLoader;
      _uiFactory = uiFactory;
      _progressService = progressService;
      _saveLoadService = saveLoadService;
      _gameFactory = gameFactory;
      _currencyService = currencyService;
      _lives = lives;
    }

    public void Enter(GameResultPayload payload)
    {
      _leaving = false;
      if (!_resultCommitted)
      {
        _isVictory = payload.IsVictory;
        ApplyResult(payload);
      }
#if UNITY_EDITOR
      GameLogger.Message($"[GameResultState] Result received: victory={_isVictory}, level index {_progressService.PlayerProgress.LevelIndex}.");
#endif


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

    private void ApplyResult(GameResultPayload payload)
    {
      if (_resultCommitted) return;
      _isVictory = payload.IsVictory;
      var progress = _progressService.PlayerProgress;
      var config = _gameFactory.CurrentLevelConfig;
      if (config == null || _gameFactory.CurrentLevelIndex < 0 ||
          progress.LevelIndex != _gameFactory.CurrentLevelIndex)
        throw new InvalidOperationException("Cannot apply result without the current level configuration.");

      int target = Math.Max(0, config.WinCondition);
      int fullCoins = CoinsRewardFor(config.Difficulty);
      _hexCoinsReward = _isVictory ? target : 0;
      _coinsReward = _isVictory ? fullCoins : 0;

      // Bring regeneration up to date before the result transaction.
      _lives.Refresh();
      _saveLoadService.ExecuteTransaction(() =>
      {
        if (_isVictory)
        {
          progress.LevelIndex++;
          _currencyService.AddCoins(_coinsReward);
          _currencyService.AddHexCoins(_hexCoinsReward);
        }
        else
          _lives.TrySpendLife();
      });
      _resultCommitted = true;
#if UNITY_EDITOR
      GameLogger.Message($"[GameResultState] Result saved: victory={_isVictory}, " +
        $"Coins=+{_coinsReward}, HexCoins=+{_hexCoinsReward}, lives={_lives.Lives}.");
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

      _popup.SetRewards(_coinsReward, _hexCoinsReward);
      if (_isVictory)
        _popup.ShowVictory();
      else
        _popup.ShowDefeat();
    }

    private void HandleContinueClicked()
    {
      if (_leaving) return;
      _leaving = true;
#if UNITY_EDITOR
      GameLogger.Message($"[GameResultState] Continue requested: victory={_isVictory}.");
#endif
      // Victory was already committed on entry. Defeat keeps the same index.
      ReloadGameScene();
    }

    private void HandleMainMenuClicked()
    {
      if (_leaving) return;
      _leaving = true;
      _gameStateMachine.Enter<MainMenuState>();
    }

    private void ReloadGameScene() =>
      _gameStateMachine.Enter<LoadLevelState, string>(
        _progressService.PlayerProgress.WorldData.LastLevel.Level);
  }
}
