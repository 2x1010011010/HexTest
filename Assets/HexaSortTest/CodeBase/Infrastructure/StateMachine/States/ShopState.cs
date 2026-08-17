using HexaSortTest.CodeBase.GameLogic.UI.Loading;
using HexaSortTest.CodeBase.GameLogic.UI.Shop;
using HexaSortTest.CodeBase.Infrastructure.Services.Factories;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States
{
  public class ShopState : IState
  {
    private readonly GameStateMachine _gameStateMachine;
    private readonly SceneLoader _sceneLoader;
    private readonly LoadingCurtain _loadingCurtain;
    private readonly IUIFactory _uiFactory;

    private ShopSceneObserver _shopUI;

    public ShopState(
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
      _sceneLoader.Load(Constants.ShopScene, onLoaded: SpawnShopUI);
    }

    public void Exit()
    {
      if (_shopUI != null)
        _shopUI.OnExitRequested -= HandleExitRequested;

      _shopUI = null;
      _uiFactory.Clear();
    }

    private void SpawnShopUI()
    {
      _loadingCurtain.Hide();

      _shopUI = _uiFactory.CreateShopSceneUI();

      if (_shopUI == null)
      {
        Debug.LogError("[ShopState] Failed to spawn ShopSceneObserver. " +
                       "Check that AssetPaths.ShopSceneUIPrefab points to a valid prefab under Resources/.");
        return;
      }

      _shopUI.OnExitRequested += HandleExitRequested;
    }

    private void HandleExitRequested() =>
      _gameStateMachine.Enter<MainMenuState>();
  }
}