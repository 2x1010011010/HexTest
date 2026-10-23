using HexaSortTest.CodeBase.GameLogic.UI.Loading;
using Sirenix.OdinInspector;
using HexaSortTest.CodeBase.Infrastructure.Logging;
using UnityEngine;
using Zenject;

namespace HexaSortTest.CodeBase.Infrastructure.DI
{
  public class BootstrapSceneInstaller : MonoInstaller
  {
    [SerializeField, BoxGroup("SCENE REFERENCES")] private LoadingCurtain _loadingCurtain;
    [SerializeField, BoxGroup("SCENE REFERENCES")] private Bootstrapper _bootstrapper;

    public override void InstallBindings()
    {
#if UNITY_EDITOR
      GameLogger.Message("[DI][BootstrapSceneInstaller] Installing bindings.");
#endif
      Container
        .Bind<LoadingCurtain>()
        .FromInstance(_loadingCurtain)
        .AsSingle();
#if UNITY_EDITOR
      GameLogger.Message("[DI][BootstrapSceneInstaller] Binding registered: LoadingCurtain.");
#endif

      Container
        .Bind<ICoroutineRunner>()
        .FromInstance(_bootstrapper)
        .AsSingle();
#if UNITY_EDITOR
      GameLogger.Message("[DI][BootstrapSceneInstaller] Binding registered: ICoroutineRunner.");
#endif

      Container
        .Bind<Game>()
        .AsSingle();
#if UNITY_EDITOR
      GameLogger.Message("[DI][BootstrapSceneInstaller] Binding registered: Game.");
#endif
#if UNITY_EDITOR
      GameLogger.Message("[DI][BootstrapSceneInstaller] Binding installation complete.");
#endif
    }
  }
}