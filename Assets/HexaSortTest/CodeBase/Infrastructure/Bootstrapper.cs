using HexaSortTest.CodeBase.GameLogic.UI.Loading;
using HexaSortTest.CodeBase.Infrastructure.StateMachine.States;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure
{
  public sealed class Bootstrapper : MonoBehaviour, ICoroutineRunner
  {
    [SerializeField, BoxGroup("SETUP")] private LoadingCurtain _loadingCurtain;
    
    private Game _game;

    private void Awake()
    {
      _game = new Game(this, _loadingCurtain);
      _game.StateMachine.Enter<BootstrapState>();
      
      DontDestroyOnLoad(this);
    }
  }
}