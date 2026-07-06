using HexaSortTest.CodeBase.GameLogic.UI.ResultPopup;
using HexaSortTest.CodeBase.Infrastructure.Services.AssetManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace HexaSortTest.CodeBase.Infrastructure.Services.Factories
{
  public class GameResultFactory : IGameResultFactory
  {
    private readonly DiContainer _container;
    private GameObject _instance;

    [Inject]
    public GameResultFactory(DiContainer container) =>
      _container = container;

    public GameResultPopup CreateGameResultPopup()
    {
      Clear();

      var prefab = Resources.Load<GameObject>(AssetPaths.GameResultPopupPrefab);
      if (prefab == null)
      {
        Debug.LogError($"[GameResultFactory] Prefab not found at Resources/{AssetPaths.GameResultPopupPrefab}");
        return null;
      }

      _instance = _container.InstantiatePrefab(prefab);
      _instance.transform.SetParent(null);
      SceneManager.MoveGameObjectToScene(_instance, SceneManager.GetActiveScene());

      var popup = _instance.GetComponent<GameResultPopup>();
      if (popup == null)
        Debug.LogError($"[GameResultFactory] Spawned prefab at {AssetPaths.GameResultPopupPrefab} has no GameResultPopup component!");

      return popup;
    }

    public void Clear()
    {
      if (_instance != null)
        Object.Destroy(_instance);

      _instance = null;
    }
  }
}