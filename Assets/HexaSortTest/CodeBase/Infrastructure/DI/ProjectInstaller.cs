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
        .Bind<IUIFactory>()
        .To<UIFactory>()
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