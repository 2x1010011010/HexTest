using HexaSortTest.CodeBase.GameLogic.Spawners;
using HexaSortTest.CodeBase.GameLogic.UI.HUD;
using HexaSortTest.CodeBase.GameLogic.UI.MainMenu;
using Sirenix.OdinInspector;
using HexaSortTest.CodeBase.Infrastructure.Logging;
using UnityEngine;
using Zenject;

namespace HexaSortTest.CodeBase.Infrastructure.DI
{
  public class GameSceneInstaller : MonoInstaller
  {
    [SerializeField, BoxGroup("SCENE REFERENCES")] private HudObserver _hudObserver;
    [SerializeField, BoxGroup("SCENE REFERENCES")] private MainMenuObserver _mainMenuObserver;
    [SerializeField, BoxGroup("SCENE REFERENCES")] private StacksSpawner _stacksSpawner;

    public override void InstallBindings()
    {
#if UNITY_EDITOR
      GameLogger.Message("[DI][GameSceneInstaller] Installing bindings.", this);
#endif
      if (_hudObserver != null)
      {
        Container
          .Bind<HudObserver>()
          .FromInstance(_hudObserver)
          .AsSingle();
#if UNITY_EDITOR
        GameLogger.Message("[DI][GameSceneInstaller] Binding registered: HudObserver.", this);
#endif
      }
#if UNITY_EDITOR
      else
      {
        GameLogger.Warning("[DI][GameSceneInstaller] _hudObserver is not assigned; its binding will be skipped.", this);
      }
#endif

      if (_mainMenuObserver != null)
      {
        Container
          .Bind<MainMenuObserver>()
          .FromInstance(_mainMenuObserver)
          .AsSingle();
#if UNITY_EDITOR
        GameLogger.Message("[DI][GameSceneInstaller] Binding registered: MainMenuObserver.", this);
#endif
      }
#if UNITY_EDITOR
      else
      {
        GameLogger.Warning("[DI][GameSceneInstaller] _mainMenuObserver is not assigned; its binding will be skipped.", this);
      }
#endif

      if (_stacksSpawner != null)
      {
        Container
          .Bind<StacksSpawner>()
          .FromInstance(_stacksSpawner)
          .AsSingle();
#if UNITY_EDITOR
        GameLogger.Message("[DI][GameSceneInstaller] Binding registered: StacksSpawner.", this);
#endif
      }
#if UNITY_EDITOR
      else
      {
        GameLogger.Warning("[DI][GameSceneInstaller] _stacksSpawner is not assigned; its binding will be skipped.", this);
      }
#endif

#if UNITY_EDITOR
      GameLogger.Message("[DI][GameSceneInstaller] Binding installation complete.", this);
#endif
    }
  }
}
