using HexaSortTest.CodeBase.Infrastructure.Services;

namespace HexaSortTest.CodeBase.Infrastructure
{
  public sealed class Game
  {
    public StateMachine.StateMachine StateMachine;

    public Game(ICoroutineRunner coroutineRunner) => 
      StateMachine = new StateMachine.StateMachine(new SceneLoader(coroutineRunner), ServiceLocator.Container);
  }
}