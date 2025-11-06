using CodeBase.Infrastructure;
using CodeBase.Infrastructure.Services.AssetManagement;
using CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.GameLogic.UI.Loading;
using HexaSortTest.CodeBase.Infrastructure.Services;
using HexaSortTest.CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.Infrastructure.Services.InputService;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using HexaSortTest.CodeBase.Infrastructure.Services.SaveAndLoadService;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States
{
  public class BootstrapState : IState
  {
    private readonly GameStateMachine _gameStateMachine;
    private readonly SceneLoader _sceneLoader;
    private readonly ServiceLocator _serviceLocator;
    private readonly LoadingCurtain _loadingCurtain;

    public BootstrapState(GameStateMachine gameStateMachine, SceneLoader sceneLoader, ServiceLocator serviceLocator, LoadingCurtain loadingCurtain)
    {
      _gameStateMachine = gameStateMachine;
      _sceneLoader = sceneLoader;
      _serviceLocator = serviceLocator;
      _loadingCurtain = loadingCurtain;

      RegisterServices();
    }

    public void Enter()
    {
      _loadingCurtain.Show();
      _sceneLoader.Load(sceneName: Constants.InitialScene, onLoaded: EnterLoadLevel);
    }

    public void Exit()
    {
    }

    private void EnterLoadLevel() =>
      _gameStateMachine.Enter<LoadProgressState>();

    private void RegisterServices()
    {
      _serviceLocator.RegisterSingle<IInputService>(InputService());
      _serviceLocator.RegisterSingle<IAssetProvider>(new AssetProvider());
      _serviceLocator.RegisterSingle<IPersistentProgressService>(new PersistentProgressService());
      _serviceLocator.RegisterSingle<IGameFactory>(new GameFactory(_serviceLocator.Single<IAssetProvider>()));
      _serviceLocator.RegisterSingle<ISaveLoadService>(new SaveLoadService(_serviceLocator.Single<IPersistentProgressService>(), _serviceLocator.Single<IGameFactory>()));
    }

    private static IInputService InputService()
    {
      if (Application.isMobilePlatform)
        return new MobileInputService();
      else
        return new DesktopInputService();
    }
  }
}