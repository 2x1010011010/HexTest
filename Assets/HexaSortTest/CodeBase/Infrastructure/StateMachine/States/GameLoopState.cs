using HexaSortTest.CodeBase.GameLogic.UI.Loading;
using HexaSortTest.CodeBase.Infrastructure.Services.UIService;

namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States
{
  public class GameLoopState : IState
  {
    private readonly GameStateMachine _gameStateMachine;
    private readonly LoadingCurtain _loadingCurtain;
    private readonly IUIListenerService _uiListenerService;

    public GameLoopState(GameStateMachine gameStateMachine, LoadingCurtain curtain, IUIListenerService uiListenerService)
    {
      _gameStateMachine = gameStateMachine;
      _loadingCurtain = curtain;
      _uiListenerService = uiListenerService;
    }

    public void Enter()
    {
      _loadingCurtain.Hide();
      _uiListenerService.ActionRequired += ClearScene;
    }

    public void Exit()
    {
      _uiListenerService.ActionRequired -= ClearScene;
    }

    private void ClearScene()
    {
    }
  }
}