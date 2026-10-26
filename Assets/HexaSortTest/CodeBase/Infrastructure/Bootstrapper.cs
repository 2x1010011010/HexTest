using HexaSortTest.CodeBase.GameLogic.UI.Loading;
using HexaSortTest.CodeBase.Infrastructure.Logging;
using HexaSortTest.CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.Infrastructure.StateMachine.States;
using HexaSortTest.CodeBase.Infrastructure.Services.SaveAndLoadService;
using UnityEngine;
using Zenject;

namespace HexaSortTest.CodeBase.Infrastructure
{
  public sealed class Bootstrapper : MonoBehaviour, ICoroutineRunner
  {
    [Inject] private Game _game;
    [Inject] private ISaveLoadService _saveLoadService;

    private void OnApplicationPause(bool paused)
    {
      if (paused) _saveLoadService?.SaveProgress();
    }

    private void OnApplicationFocus(bool focused)
    {
      if (!focused) _saveLoadService?.SaveProgress();
    }

    private void OnApplicationQuit() => _saveLoadService?.SaveProgress();

    private void Awake()
    {
#if UNITY_EDITOR
      GameLogger.Message("[Bootstrapper] Starting game initialization.", this);
#endif
      Application.targetFrameRate = 60;
      Screen.orientation = ScreenOrientation.Portrait;
      Input.multiTouchEnabled = false;
      
      
      _game.StateMachine.Enter<BootstrapState>();
      
      DontDestroyOnLoad(this);
#if UNITY_EDITOR
      GameLogger.Message("[Bootstrapper] Bootstrapper marked persistent.", this);
#endif
    }
  }
}