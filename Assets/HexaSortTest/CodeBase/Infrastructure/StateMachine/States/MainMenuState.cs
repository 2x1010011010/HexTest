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
    private readonly IMainMenuFactory _menuFactory;
    private readonly IPersistentProgressService _progressService;

    private MainMenuScreen _screen;

    public MainMenuState(
      GameStateMachine gameStateMachine,
      SceneLoader sceneLoader,
      LoadingCurtain loadingCurtain,
      IMainMenuFactory menuFactory,
      IPersistentProgressService progressService)
    {
      _gameStateMachine = gameStateMachine;
      _sceneLoader = sceneLoader;
      _loadingCurtain = loadingCurtain;
      _menuFactory = menuFactory;
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
      _menuFactory.Clear();
    }

    private void SpawnScreen()
    {
      _loadingCurtain.Hide();

      _screen = _menuFactory.CreateMainMenuScreen();

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
      _loadingCurtain.Show();
      _gameStateMachine.Enter<LoadLevelState, string>(
        _progressService.PlayerProgress.WorldData.LastLevel.Level);
    }
  }
}
