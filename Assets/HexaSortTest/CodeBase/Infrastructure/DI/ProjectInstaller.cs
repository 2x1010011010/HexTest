using HexaSortTest.CodeBase.Infrastructure.Services.AssetManagement;
using HexaSortTest.CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.Infrastructure.Services.InputService;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using HexaSortTest.CodeBase.Infrastructure.Services.SaveAndLoadService;
using HexaSortTest.CodeBase.Infrastructure.Services.UIService;
using UnityEngine;
using Zenject;

namespace HexaSortTest.CodeBase.Infrastructure.DI
{
  /// <summary>
  /// Project-context installer — lives in Resources/ProjectContext and runs
  /// exactly once for the whole application lifetime.
  ///
  /// Binds every pure-C# service. MonoBehaviour dependencies that can only be
  /// known from a scene (LoadingCurtain, ICoroutineRunner/Bootstrapper) are
  /// bound in <see cref="BootstrapSceneInstaller"/>.
  /// </summary>
  [CreateAssetMenu(
    fileName = "ProjectInstaller",
    menuName = "Installers/ProjectInstaller")]
  public class ProjectInstaller : ScriptableObjectInstaller<ProjectInstaller>
  {
    public override void InstallBindings()
    {
      Container
        .Bind<IAssetProvider>()
        .To<AssetProvider>()
        .AsSingle();
      
      Container
        .Bind<IPersistentProgressService>()
        .To<PersistentProgressService>()
        .AsSingle();

      Container
        .Bind<ISaveLoadService>()
        .To<SaveLoadService>()
        .AsSingle();

      Container
        .Bind<IGameFactory>()
        .To<GameFactory>()
        .AsSingle();

      Container
        .Bind<IUIListenerService>()
        .To<RestartLevelService>()
        .AsSingle();

      Container
        .Bind<IInputService>()
        .FromMethod(_ => Application.isMobilePlatform
          ? (IInputService)new MobileInputService()
          : new DesktopInputService())
        .AsSingle();
    }
  }
}