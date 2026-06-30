using HexaSortTest.CodeBase.GameLogic.UI.HUD;
using HexaSortTest.CodeBase.Infrastructure.Services.GameResultService;
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
    private readonly IGameResultPopupRegistry _popupRegistry;
    private readonly IPersistentProgressService _progressService;
    private readonly ISaveLoadService _saveLoadService;

    private GameResultPopup _popup;
    private bool _isVictory;

    public GameResultState(
      GameStateMachine gameStateMachine,
      SceneLoader sceneLoader,
      IGameResultPopupRegistry popupRegistry,
      IPersistentProgressService progressService,
      ISaveLoadService saveLoadService)
    {
      _gameStateMachine = gameStateMachine;
      _sceneLoader = sceneLoader;
      _popupRegistry = popupRegistry;
      _progressService = progressService;
      _saveLoadService = saveLoadService;
    }

    public void Enter(GameResultPayload payload)
    {
      _isVictory = payload.IsVictory;
      _popupRegistry.Clear();

      _sceneLoader.Load(Constants.GameResultScene, onLoaded: ShowPopup);
    }

    public void Exit()
    {
      if (_popup != null)
        _popup.OnContinueClicked -= HandleContinueClicked;

      _popup = null;
      _popupRegistry.Clear();
    }

    private void ShowPopup()
    {
      _popup = _popupRegistry.Popup;

      if (_popup == null)
      {
        Debug.LogError("[GameResultState] GameResultPopup not found in registry after loading GameResult scene. " +
                        "Check that GameResultSceneInstaller is present in the scene and has _resultPopup assigned.");
        return;
      }

      _popup.OnContinueClicked += HandleContinueClicked;

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
