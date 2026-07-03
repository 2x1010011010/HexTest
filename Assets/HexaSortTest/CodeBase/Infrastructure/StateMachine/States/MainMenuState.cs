using HexaSortTest.CodeBase.GameLogic.UI.Loading;
using HexaSortTest.CodeBase.GameLogic.UI.Menu;
using HexaSortTest.CodeBase.Infrastructure.Services.MainMenuService;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States
{
  public class MainMenuState : IState
  {
    private readonly GameStateMachine _gameStateMachine;
    private readonly SceneLoader _sceneLoader;
    private readonly LoadingCurtain _loadingCurtain;
    private readonly IMainMenuRegistry _menuRegistry;
    private readonly IPersistentProgressService _progressService;

    private MainMenuScreen _screen;

    public MainMenuState(
      GameStateMachine gameStateMachine,
      SceneLoader sceneLoader,
      LoadingCurtain loadingCurtain,
      IMainMenuRegistry menuRegistry,
      IPersistentProgressService progressService)
    {
      _gameStateMachine = gameStateMachine;
      _sceneLoader = sceneLoader;
      _loadingCurtain = loadingCurtain;
      _menuRegistry = menuRegistry;
      _progressService = progressService;
    }

    public void Enter()
    {
      _loadingCurtain.Show();
      _menuRegistry.Clear();
      _sceneLoader.Load(Constants.MainMenuScene, onLoaded: ShowScreen);
    }

    public void Exit()
    {
      if (_screen != null)
        _screen.OnPlayClicked -= HandlePlayClicked;

      _screen = null;
      _menuRegistry.Clear();
    }

    private void ShowScreen()
    {
      _loadingCurtain.Hide();

      _screen = _menuRegistry.Screen;

      if (_screen == null)
      {
        Debug.LogError("[MainMenuState] MainMenuScreen not found in registry after loading MainMenu scene. " +
                        "Check that MainMenuSceneInstaller is present in the scene and has _mainMenuScreen assigned.");
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
