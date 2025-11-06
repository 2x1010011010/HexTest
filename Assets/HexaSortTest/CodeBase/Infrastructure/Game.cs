using HexaSortTest.CodeBase.GameLogic.UI.Loading;
using HexaSortTest.CodeBase.Infrastructure.Services;
using HexaSortTest.CodeBase.Infrastructure.StateMachine;

namespace HexaSortTest.CodeBase.Infrastructure
{
  public sealed class Game
  {
    public readonly GameStateMachine StateMachine; 

    public Game(ICoroutineRunner coroutineRunner, LoadingCurtain curtain) => 
      StateMachine = new GameStateMachine(new SceneLoader(coroutineRunner), ServiceLocator.Container, curtain);
  }
}