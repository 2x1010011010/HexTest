using HexaSortTest.CodeBase.GameLogic.UI.Loading;
using HexaSortTest.CodeBase.Infrastructure.StateMachine.States;
using UnityEngine;
using Zenject;

namespace HexaSortTest.CodeBase.Infrastructure
{
  public sealed class Bootstrapper : MonoBehaviour, ICoroutineRunner
  {
    [Inject] private Game _game;

    private void Awake()
    {
      Application.targetFrameRate = 60;
      Screen.orientation = ScreenOrientation.Portrait;
      Input.multiTouchEnabled = false;
      
      _game.StateMachine.Enter<BootstrapState>();
      
      DontDestroyOnLoad(this);
    }
  }
}