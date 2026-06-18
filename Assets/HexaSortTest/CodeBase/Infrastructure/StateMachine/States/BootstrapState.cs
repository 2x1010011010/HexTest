using HexaSortTest.CodeBase.GameLogic.UI.Loading;
using HexaSortTest.CodeBase.Infrastructure.Services;
using HexaSortTest.CodeBase.Infrastructure.Services.AssetManagement;
using HexaSortTest.CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.Infrastructure.Services.InputService;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using HexaSortTest.CodeBase.Infrastructure.Services.SaveAndLoadService;
using HexaSortTest.CodeBase.Infrastructure.Services.UIService;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States
{
  public class BootstrapState : IState
  {
    private readonly GameStateMachine _gameStateMachine;
    private readonly SceneLoader _sceneLoader;
    private readonly LoadingCurtain _loadingCurtain;

    public BootstrapState(
      GameStateMachine gameStateMachine,
      SceneLoader      sceneLoader,
      LoadingCurtain   loadingCurtain)
    {
      _gameStateMachine = gameStateMachine;
      _sceneLoader      = sceneLoader;
      _loadingCurtain   = loadingCurtain;
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
  }
}