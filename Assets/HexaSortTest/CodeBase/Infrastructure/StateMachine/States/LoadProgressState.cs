using CodeBase.Infrastructure;
using HexaSortTest.CodeBase.GameLogic.Data;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using HexaSortTest.CodeBase.Infrastructure.Services.SaveAndLoadService;

namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States
{
  public class LoadProgressState : IState
  {
    private readonly StateMachine _stateMachine;
    private readonly IPersistentProgressService _progressService;
    private readonly ISaveLoadService _saveLoadService;

    public LoadProgressState(StateMachine stateMachine, IPersistentProgressService progressService, ISaveLoadService saveLoadService)
    {
      _stateMachine = stateMachine;
      _progressService = progressService;
      _saveLoadService = saveLoadService;
    }

    public void Enter()
    {
      LoadProgressOrInitNew();
      _stateMachine.Enter<LoadLevelState, string>(_progressService.PlayerProgress.WorldData.PositionOnLevel.Level);
    }

    public void Exit()
    {
    }

    private void LoadProgressOrInitNew() => 
      _progressService.PlayerProgress = 
        _saveLoadService.LoadProgress() 
        ?? NewProgress();

    private PlayerProgress NewProgress() => 
      new PlayerProgress(Constants.GameScene);
  }
}