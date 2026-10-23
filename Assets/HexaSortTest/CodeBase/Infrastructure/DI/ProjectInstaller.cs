using HexaSortTest.CodeBase.Infrastructure.Logging;
using HexaSortTest.CodeBase.Infrastructure.Services.AssetManagement;
using HexaSortTest.CodeBase.Infrastructure.Services.BoosterInventoryService;
using HexaSortTest.CodeBase.Infrastructure.Services.CurrencyService;
using HexaSortTest.CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.Infrastructure.Services.InputService;
using HexaSortTest.CodeBase.Infrastructure.Services.MetaProgressService;
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
#if UNITY_EDITOR
      GameLogger.Message("[DI][ProjectInstaller] Installing bindings.");
#endif
      Container
        .Bind<IAssetProvider>()
        .To<AssetProvider>()
        .AsSingle();
#if UNITY_EDITOR
      GameLogger.Message("[DI][ProjectInstaller] Binding registered: IAssetProvider.");
#endif

      Container
        .Bind<IPersistentProgressService>()
        .To<PersistentProgressService>()
        .AsSingle();
#if UNITY_EDITOR
      GameLogger.Message("[DI][ProjectInstaller] Binding registered: IPersistentProgressService.");
#endif

      Container
        .Bind<ISaveLoadService>()
        .To<SaveLoadService>()
        .AsSingle();
#if UNITY_EDITOR
      GameLogger.Message("[DI][ProjectInstaller] Binding registered: ISaveLoadService.");
#endif

      Container
        .Bind<IGameFactory>()
        .To<GameFactory>()
        .AsSingle();
#if UNITY_EDITOR
      GameLogger.Message("[DI][ProjectInstaller] Binding registered: IGameFactory.");
#endif

      Container
        .Bind<IUIFactory>()
        .To<UIFactory>()
        .AsSingle();
#if UNITY_EDITOR
      GameLogger.Message("[DI][ProjectInstaller] Binding registered: IUIFactory.");
#endif

      Container
        .Bind<IUIListenerService>()
        .To<RestartLevelService>()
        .AsSingle();
#if UNITY_EDITOR
      GameLogger.Message("[DI][ProjectInstaller] Binding registered: IUIListenerService.");
#endif

      Container
        .Bind<IMetaObserver>()
        .To<MetaObserver>()
        .AsSingle();
#if UNITY_EDITOR
      GameLogger.Message("[DI][ProjectInstaller] Binding registered: IMetaObserver.");
#endif

      Container
        .Bind<IMetaObjectFactory>()
        .To<MetaObjectFactory>()
        .AsSingle();
#if UNITY_EDITOR
      GameLogger.Message("[DI][ProjectInstaller] Binding registered: IMetaObjectFactory.");
#endif

      Container
        .Bind<ICurrencyService>()
        .To<CurrencyService>()
        .AsSingle();
#if UNITY_EDITOR
      GameLogger.Message("[DI][ProjectInstaller] Binding registered: ICurrencyService.");
#endif

      Container
        .Bind<IBoosterInventoryService>()
        .To<BoosterInventoryService>()
        .AsSingle();
#if UNITY_EDITOR
      GameLogger.Message("[DI][ProjectInstaller] Binding registered: IBoosterInventoryService.");
#endif

      Container
        .Bind<IInputService>()
        .FromMethod(_ => Application.isMobilePlatform
          ? (IInputService)new MobileInputService()
          : new DesktopInputService())
        .AsSingle();
#if UNITY_EDITOR
      GameLogger.Message("[DI][ProjectInstaller] Binding registered: IInputService.");
#endif
#if UNITY_EDITOR
      GameLogger.Message("[DI][ProjectInstaller] Binding installation complete.");
#endif
    }
  }
}
