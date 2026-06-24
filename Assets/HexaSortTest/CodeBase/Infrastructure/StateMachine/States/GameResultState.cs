using HexaSortTest.CodeBase.GameLogic.UI.HUD;
using HexaSortTest.CodeBase.GameLogic.UI.ResultPopup;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using HexaSortTest.CodeBase.Infrastructure.Services.SaveAndLoadService;
using HexaSortTest.CodeBase.Infrastructure.StateMachine.States.CustomPayloadStructures;

namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States
{
  public class GameResultState : IPayloadState<GameResultPayload>
  {
    private readonly GameStateMachine _gameStateMachine;
    private readonly IPersistentProgressService _progressService;
    private readonly ISaveLoadService _saveLoadService;

    private GameResultPopup _popup;
    private bool _isVictory;

    public GameResultState(
      GameStateMachine gameStateMachine,
      IPersistentProgressService progressService,
      ISaveLoadService saveLoadService)
    {
      _gameStateMachine = gameStateMachine;
      _progressService = progressService;
      _saveLoadService = saveLoadService;
    }

    public void Enter(GameResultPayload payload)
    {
      _popup = payload.Popup;
      _isVictory = payload.IsVictory;

      _popup.OnContinueClicked += HandleContinueClicked;

      if (_isVictory)
        _popup.ShowVictory();
      else
        _popup.ShowDefeat();
    }

    public void Exit()
    {
      if (_popup != null)
        _popup.OnContinueClicked -= HandleContinueClicked;
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