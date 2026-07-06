using HexaSortTest.CodeBase.GameLogic.UI.ResultPopup;
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
    private readonly IGameResultFactory _resultFactory;
    private readonly IPersistentProgressService _progressService;
    private readonly ISaveLoadService _saveLoadService;

    private GameResultPopup _popup;
    private bool _isVictory;

    public GameResultState(
      GameStateMachine gameStateMachine,
      SceneLoader sceneLoader,
      IGameResultFactory resultFactory,
      IPersistentProgressService progressService,
      ISaveLoadService saveLoadService)
    {
      _gameStateMachine = gameStateMachine;
      _sceneLoader = sceneLoader;
      _resultFactory = resultFactory;
      _progressService = progressService;
      _saveLoadService = saveLoadService;
    }

    public void Enter(GameResultPayload payload)
    {
      _isVictory = payload.IsVictory;

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
      _resultFactory.Clear();
    }

    private void SpawnPopup()
    {
      _popup = _resultFactory.CreateGameResultPopup();

      if (_popup == null)
      {
        Debug.LogError("[GameResultState] Failed to spawn GameResultPopup. " +
                        "Check that AssetPaths.GameResultPopupPrefab points to a valid prefab under Resources/.");
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
      if (_isVictory)
        AdvanceToNextLevel();
      else
        RestartCurrentLevel();
    }

    private void HandleMainMenuClicked() =>
      _gameStateMachine.Enter<MainMenuState>();

    private void AdvanceToNextLevel()
    {
      _progressService.PlayerProgress.LevelIndex++;
      _saveLoadService.SaveProgress();

      ReloadGameScene();
    }

    private void RestartCurrentLevel() =>
      ReloadGameScene();

    private void ReloadGameScene() =>
      _gameStateMachine.Enter<LoadLevelState, string>(
        _progressService.PlayerProgress.WorldData.LastLevel.Level);
  }
}
