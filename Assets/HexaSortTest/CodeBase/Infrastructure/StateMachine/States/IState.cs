namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States
{
  public interface IState : IExitState
  {
    void Enter();
  }
}