using System.Collections.Generic;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using HexaSortTest.CodeBase.GameLogic.Meta;
using HexaSortTest.CodeBase.GameLogic.Spawners;
using HexaSortTest.CodeBase.GameLogic.UI.HUD;
using HexaSortTest.CodeBase.GameLogic.UI.MainMenu;
using HexaSortTest.CodeBase.GameLogic.UI.Menu;
using HexaSortTest.CodeBase.GameLogic.UI.Meta;
using HexaSortTest.CodeBase.GameLogic.UI.ResultPopup;
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

    public void CreateHud(int winCondition, MainMenuObserver mainMenu, StacksSpawner stacksSpawner, GridObserver gridObserver)
    {
      var go = InstantiateInjected(AssetPaths.HUD);
      go.GetComponent<HudObserver>().Init(winCondition, mainMenu, stacksSpawner, gridObserver);
    }

    public MainMenuScreen CreateMainMenuScreen()
    {
      var prefab = Resources.Load<GameObject>(AssetPaths.MainMenuScreenPrefab);
      if (prefab == null)
      {
        Debug.LogError($"[UIFactory] Prefab not found at Resources/{AssetPaths.MainMenuScreenPrefab}");
        return null;
      }

      var go = InstantiateInjected(prefab);

      var screen = go.GetComponent<MainMenuScreen>();
      if (screen == null)
        Debug.LogError($"[UIFactory] Spawned prefab at {AssetPaths.MainMenuScreenPrefab} has no MainMenuScreen component!");

      return screen;
    }

    public GameResultPopup CreateGameResultPopup()
    {
      var prefab = Resources.Load<GameObject>(AssetPaths.GameResultPopupPrefab);
      if (prefab == null)
      {
        Debug.LogError($"[UIFactory] Prefab not found at Resources/{AssetPaths.GameResultPopupPrefab}");
        return null;
      }

      var go = InstantiateInjected(prefab);

      var popup = go.GetComponent<GameResultPopup>();
      if (popup == null)
        Debug.LogError($"[UIFactory] Spawned prefab at {AssetPaths.GameResultPopupPrefab} has no GameResultPopup component!");

      return popup;
    }

    public MetaUIObserver CreateMetaUI()
    {
      var prefab = Resources.Load<GameObject>(AssetPaths.MetaUIPrefab);
      if (prefab == null)
      {
        Debug.LogError($"[UIFactory] Prefab not found at Resources/{AssetPaths.MetaUIPrefab}");
        return null;
      }

      var go = InstantiateInjected(prefab);

      var metaUI = go.GetComponent<MetaUIObserver>();
      if (metaUI == null)
        Debug.LogError($"[UIFactory] Spawned prefab at {AssetPaths.MetaUIPrefab} has no MetaUIObserver component!");

      return metaUI;
    }

    public void Clear()
    {
      foreach (var go in _instances)
        if (go != null)
          Object.Destroy(go);

      _instances.Clear();
    }

    private GameObject InstantiateInjected(string resourcePath) =>
      InstantiateInjected(Resources.Load<GameObject>(resourcePath));

    private GameObject InstantiateInjected(GameObject prefab)
    {
      var go = _container.InstantiatePrefab(prefab);
      go.transform.SetParent(null);
      SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
      _instances.Add(go);
      return go;
    }
  }
}
