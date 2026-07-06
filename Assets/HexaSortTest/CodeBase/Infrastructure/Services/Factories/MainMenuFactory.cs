using HexaSortTest.CodeBase.GameLogic.UI.Menu;
using HexaSortTest.CodeBase.Infrastructure.Services.AssetManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace HexaSortTest.CodeBase.Infrastructure.Services.Factories
{
  public class MainMenuFactory : IMainMenuFactory
  {
    private readonly DiContainer _container;
    private GameObject _instance;

    [Inject]
    public MainMenuFactory(DiContainer container) =>
      _container = container;

    public MainMenuScreen CreateMainMenuScreen()
    {
      Clear();

      var prefab = Resources.Load<GameObject>(AssetPaths.MainMenuScreenPrefab);
      if (prefab == null)
      {
        Debug.LogError($"[MainMenuFactory] Prefab not found at Resources/{AssetPaths.MainMenuScreenPrefab}");
        return null;
      }

      _instance = _container.InstantiatePrefab(prefab);
      _instance.transform.SetParent(null);
      SceneManager.MoveGameObjectToScene(_instance, SceneManager.GetActiveScene());

      var screen = _instance.GetComponent<MainMenuScreen>();
      if (screen == null)
        Debug.LogError($"[MainMenuFactory] Spawned prefab at {AssetPaths.MainMenuScreenPrefab} has no MainMenuScreen component!");

      return screen;
    }

    public void Clear()
    {
      if (_instance != null)
        Object.Destroy(_instance);

      _instance = null;
    }
  }
}