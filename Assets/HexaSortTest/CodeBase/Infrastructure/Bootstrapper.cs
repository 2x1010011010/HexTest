using HexaSortTest.CodeBase.Infrastructure.StateMachine.States;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure
{
  public sealed class Bootstrapper : MonoBehaviour, ICoroutineRunner
  {
    private Game _game;

    private void Awake()
    {
      _game = new Game(this);
      _game.StateMachine.Enter<BootstrapState>();
      
      DontDestroyOnLoad(this);
    }
  }
}