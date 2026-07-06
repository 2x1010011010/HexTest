using HexaSortTest.CodeBase.GameLogic.UI.Loading;
using HexaSortTest.CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.Infrastructure.Services.UIService;

namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States
{
  public class GameLoopState : IState
  {
    private readonly GameStateMachine _gameStateMachine;
    private readonly LoadingCurtain _loadingCurtain;
    private readonly IUIListenerService _uiListenerService;
    private readonly IGameFactory _gameFactory;
    private readonly IUIFactory _uiFactory;

    public GameLoopState(
      GameStateMachine gameStateMachine,
      LoadingCurtain curtain,
      IUIListenerService uiListenerService,
      IGameFactory gameFactory,
      IUIFactory uiFactory
    )
    {
      _gameStateMachine = gameStateMachine;
      _loadingCurtain = curtain;
      _uiListenerService = uiListenerService;
      _gameFactory = gameFactory;
      _uiFactory = uiFactory;
    }

    public void Enter()
    {
      _uiListenerService.ActionRequired += ClearScene;
      _loadingCurtain.Hide();
    }

    public void Exit()
    {
      _uiListenerService.ActionRequired -= ClearScene;
    }

    private void ClearScene()
    {
      _loadingCurtain.Show();
      _gameFactory.Clear();
      _uiFactory.Clear();
      _gameStateMachine.Enter<BootstrapState>();
    }
  }
}