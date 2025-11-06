using HexaSortTest.CodeBase.GameLogic.UI.Loading;

namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States
{
  public class GameLoopState : IState
  {
    private readonly GameStateMachine _gameStateMachine;
    private readonly LoadingCurtain _loadingCurtain;

    public GameLoopState(GameStateMachine gameStateMachine, LoadingCurtain curtain)
    {
      _gameStateMachine = gameStateMachine;
      _loadingCurtain = curtain;
    }

    public void Enter()
    {
      _loadingCurtain.Hide();
    }

    public void Exit()
    {
      
    }
  }
}