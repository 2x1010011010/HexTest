using HexaSortTest.CodeBase.GameLogic.UI.Loading;
using HexaSortTest.CodeBase.GameLogic.UI.Menu;
using HexaSortTest.CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States
{
  public class MainMenuState : IState
  {
    private readonly GameStateMachine _gameStateMachine;
    private readonly SceneLoader _sceneLoader;
    private readonly LoadingCurtain _loadingCurtain;
    private readonly IUIFactory _uiFactory;
    private readonly IPersistentProgressService _progressService;

    private MainMenuScreen _screen;

    public MainMenuState(
      GameStateMachine gameStateMachine,
      SceneLoader sceneLoader,
      LoadingCurtain loadingCurtain,
      IUIFactory uiFactory,
      IPersistentProgressService progressService)
    {
      _gameStateMachine = gameStateMachine;
      _sceneLoader = sceneLoader;
      _loadingCurtain = loadingCurtain;
      _uiFactory = uiFactory;
      _progressService = progressService;
    }

    public void Enter()
    {
      _loadingCurtain.Show();
      _sceneLoader.Load(Constants.MainMenuScene, onLoaded: SpawnScreen);
    }

    public void Exit()
    {
      if (_screen != null)
        _screen.OnPlayClicked -= HandlePlayClicked;

      _screen = null;
      _uiFactory.Clear();
    }

    private void SpawnScreen()
    {
      _loadingCurtain.Hide();

      _screen = _uiFactory.CreateMainMenuScreen();

      if (_screen == null)
      {
        Debug.LogError("[MainMenuState] Failed to spawn MainMenuScreen. " +
                        "Check that AssetPaths.MainMenuScreenPrefab points to a valid prefab under Resources/.");
        return;
      }
      
      _screen.OnPlayClicked += HandlePlayClicked;
    }

    private void HandlePlayClicked()
    {
      Debug.Log("[MainMenuState] HandlePlayClicked");
      _loadingCurtain.Show();
      _gameStateMachine.Enter<LoadLevelState, string>(
        _progressService.PlayerProgress.WorldData.LastLevel.Level);
    }
  }
}
