using System.Collections.Generic;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using HexaSortTest.CodeBase.GameLogic.Meta;
using HexaSortTest.CodeBase.GameLogic.Spawners;
using HexaSortTest.CodeBase.GameLogic.UI.HUD;
using HexaSortTest.CodeBase.GameLogic.UI.MainMenu;
using HexaSortTest.CodeBase.GameLogic.UI.Menu;
using HexaSortTest.CodeBase.GameLogic.UI.Meta;
using HexaSortTest.CodeBase.GameLogic.UI.ResultPopup;
using HexaSortTest.CodeBase.GameLogic.UI.Shop;
using HexaSortTest.CodeBase.Infrastructure.Logging;
using HexaSortTest.CodeBase.Infrastructure.Services.AssetManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace HexaSortTest.CodeBase.Infrastructure.Services.Factories
{
  public class UIFactory : IUIFactory
  {
    private readonly DiContainer _container;
    private readonly List<GameObject> _instances = new();

    [Inject]
    public UIFactory(DiContainer container) =>
      _container = container;

    public MainMenuObserver CreateMainMenu()
    {
      var go = InstantiateInjected(AssetPaths.MainMenuPath);
      return go.GetComponent<MainMenuObserver>();
    }

    public void CreateHud(int winCondition, MainMenuObserver mainMenu, StacksSpawner stacksSpawner, GridObserver gridObserver, BoosterPurchasePopup boosterPurchasePopup = null)
    {
      var go = InstantiateInjected(AssetPaths.HUD);
      go.GetComponent<HudObserver>().Init(winCondition, mainMenu, stacksSpawner, gridObserver, boosterPurchasePopup);
    }

    public MainMenuScreen CreateMainMenuScreen()
    {
      var prefab = Resources.Load<GameObject>(AssetPaths.MainMenuScreenPrefab);
      if (prefab == null)
      {
#if UNITY_EDITOR
        GameLogger.Error($"[UIFactory] Prefab not found at Resources/{AssetPaths.MainMenuScreenPrefab}");
#endif
        return null;
      }

      var go = InstantiateInjected(prefab);

      var screen = go.GetComponent<MainMenuScreen>();
      if (screen == null)
      {
#if UNITY_EDITOR
        GameLogger.Error($"[UIFactory] Spawned prefab at {AssetPaths.MainMenuScreenPrefab} has no MainMenuScreen component!");
#endif
      }

      return screen;
    }

    public GameResultPopup CreateGameResultPopup()
    {
      var prefab = Resources.Load<GameObject>(AssetPaths.GameResultPopupPrefab);
      if (prefab == null)
      {
#if UNITY_EDITOR
        GameLogger.Error($"[UIFactory] Prefab not found at Resources/{AssetPaths.GameResultPopupPrefab}");
#endif
        return null;
      }

      var go = InstantiateInjected(prefab);

      var popup = go.GetComponent<GameResultPopup>();
      if (popup == null)
      {
#if UNITY_EDITOR
        GameLogger.Error($"[UIFactory] Spawned prefab at {AssetPaths.GameResultPopupPrefab} has no GameResultPopup component!");
#endif
      }

      return popup;
    }

    public MetaUIObserver CreateMetaUI()
    {
      var prefab = Resources.Load<GameObject>(AssetPaths.MetaUIPrefab);
      if (prefab == null)
      {
#if UNITY_EDITOR
        GameLogger.Error($"[UIFactory] Prefab not found at Resources/{AssetPaths.MetaUIPrefab}");
#endif
        return null;
      }

      var go = InstantiateInjected(prefab);

      var metaUI = go.GetComponent<MetaUIObserver>();
      if (metaUI == null)
      {
#if UNITY_EDITOR
        GameLogger.Error($"[UIFactory] Spawned prefab at {AssetPaths.MetaUIPrefab} has no MetaUIObserver component!");
#endif
      }

      return metaUI;
    }

    public BoosterPurchasePopup CreateBoosterPurchasePopup()
    {
      var prefab = Resources.Load<GameObject>(AssetPaths.BoosterPurchasePopupPrefab);
      if (prefab == null)
      {
#if UNITY_EDITOR
        GameLogger.Error($"[UIFactory] Prefab not found at Resources/{AssetPaths.BoosterPurchasePopupPrefab}");
#endif
        return null;
      }

      var go = InstantiateInjected(prefab);

      var popup = go.GetComponent<BoosterPurchasePopup>();
      if (popup == null)
      {
#if UNITY_EDITOR
        GameLogger.Error($"[UIFactory] Spawned prefab at {AssetPaths.BoosterPurchasePopupPrefab} has no BoosterPurchasePopup component!");
#endif
      }

      return popup;
    }

    public ShopSceneObserver CreateShopSceneUI()
    {
      var prefab = Resources.Load<GameObject>(AssetPaths.ShopSceneUIPrefab);
      if (prefab == null)
      {
#if UNITY_EDITOR
        GameLogger.Error($"[UIFactory] Prefab not found at Resources/{AssetPaths.ShopSceneUIPrefab}");
#endif
        return null;
      }

      var go = InstantiateInjected(prefab);

      var shopUI = go.GetComponent<ShopSceneObserver>();
      if (shopUI == null)
      {
#if UNITY_EDITOR
        GameLogger.Error($"[UIFactory] Spawned prefab at {AssetPaths.ShopSceneUIPrefab} has no ShopSceneObserver component!");
#endif
      }

      return shopUI;
    }

    public void Clear()
    {
#if UNITY_EDITOR
      GameLogger.Message($"[UIFactory] Clearing {_instances.Count} UI objects.");
#endif
      foreach (var go in _instances)
        if (go != null)
          Object.Destroy(go);

      _instances.Clear();
    }

    private GameObject InstantiateInjected(string resourcePath) =>
      InstantiateInjected(Resources.Load<GameObject>(resourcePath));

    private GameObject InstantiateInjected(GameObject prefab)
    {
#if UNITY_EDITOR
      GameLogger.Message($"[UIFactory] Instantiating UI prefab {prefab?.name ?? "null"} with DI.");
#endif
      var go = _container.InstantiatePrefab(prefab);
      go.transform.SetParent(null);
      SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
      _instances.Add(go);
#if UNITY_EDITOR
      GameLogger.Message($"[UIFactory] Created {go.name} in scene {go.scene.name}.", go);
#endif
      return go;
    }
  }
}
