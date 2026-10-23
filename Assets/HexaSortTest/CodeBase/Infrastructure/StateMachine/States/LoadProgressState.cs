using HexaSortTest.CodeBase.GameLogic.Data;
using HexaSortTest.CodeBase.Infrastructure.Logging;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using HexaSortTest.CodeBase.Infrastructure.Services.SaveAndLoadService;

namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States
{
  public class LoadProgressState : IState
  {
    private readonly GameStateMachine _gameStateMachine;
    private readonly IPersistentProgressService _progressService;
    private readonly ISaveLoadService _saveLoadService;

    public LoadProgressState(
      GameStateMachine gameStateMachine,
      IPersistentProgressService progressService,
      ISaveLoadService saveLoadService)
    {
      _gameStateMachine = gameStateMachine;
      _progressService = progressService;
      _saveLoadService = saveLoadService;
    }

    public void Enter()
    {
      LoadProgressOrInitNew();
      _gameStateMachine.Enter<MainMenuState>();
    }

    public void Exit()
    {
    }

    private void LoadProgressOrInitNew()
    {
      var progress = _saveLoadService.LoadProgress();
#if UNITY_EDITOR
      GameLogger.Message(progress == null ? "[LoadProgressState] No saved progress; creating new progress." : "[LoadProgressState] Using saved progress.");
#endif
      _progressService.PlayerProgress = progress ?? NewProgress();
#if UNITY_EDITOR
      GameLogger.Message($"[LoadProgressState] Progress ready; level index {_progressService.PlayerProgress.LevelIndex}.");
#endif
    }

    private PlayerProgress NewProgress() =>
      new PlayerProgress(Constants.GameScene);
  }
}