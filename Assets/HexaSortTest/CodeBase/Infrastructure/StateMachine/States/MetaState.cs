using HexaSortTest.CodeBase.GameLogic.UI.Loading;
using HexaSortTest.CodeBase.GameLogic.UI.Meta;
using HexaSortTest.CodeBase.Infrastructure.Services.Factories;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States
{
  public class MetaState : IState
  {
    private readonly GameStateMachine _gameStateMachine;
    private readonly SceneLoader _sceneLoader;
    private readonly LoadingCurtain _loadingCurtain;
    private readonly IUIFactory _uiFactory;

    private MetaUIObserver _metaUI;

    public MetaState(
      GameStateMachine gameStateMachine,
      SceneLoader sceneLoader,
      LoadingCurtain loadingCurtain,
      IUIFactory uiFactory)
    {
      _gameStateMachine = gameStateMachine;
      _sceneLoader = sceneLoader;
      _loadingCurtain = loadingCurtain;
      _uiFactory = uiFactory;
    }

    public void Enter()
    {
      _loadingCurtain.Show();
      _sceneLoader.Load(Constants.MetaScene, onLoaded: SpawnMetaUI);
    }

    public void Exit()
    {
      if (_metaUI != null)
        _metaUI.OnExitRequested -= HandleExitRequested;

      _metaUI = null;
      _uiFactory.Clear();
    }

    private void SpawnMetaUI()
    {
      _loadingCurtain.Hide();

      _metaUI = _uiFactory.CreateMetaUI();

      if (_metaUI == null)
      {
        Debug.LogError("[MetaState] Failed to spawn MetaUIObserver. " +
                       "Check that AssetPaths.MetaUIPrefab points to a valid prefab under Resources/.");
        return;
      }

      _metaUI.OnExitRequested += HandleExitRequested;
    }

    private void HandleExitRequested() =>
      _gameStateMachine.Enter<MainMenuState>();
  }
}