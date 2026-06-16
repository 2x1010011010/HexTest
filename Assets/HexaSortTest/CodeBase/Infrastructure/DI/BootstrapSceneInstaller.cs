using HexaSortTest.CodeBase.GameLogic.UI.Loading;
using Sirenix.OdinInspector;
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
      Container
        .Bind<LoadingCurtain>()
        .FromInstance(_loadingCurtain)
        .AsSingle();

      Container
        .Bind<ICoroutineRunner>()
        .FromInstance(_bootstrapper)
        .AsSingle();

      Container
        .Bind<Game>()
        .AsSingle();
    }
  }
}