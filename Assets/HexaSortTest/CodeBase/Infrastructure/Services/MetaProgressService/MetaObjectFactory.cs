using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.Meta;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace HexaSortTest.CodeBase.Infrastructure.Services.MetaProgressService
{
  public class MetaObjectFactory : IMetaObjectFactory
  {
    private readonly DiContainer _container;
    private GameObject _currentInstance;

    public MetaTile CurrentTile { get; private set; }

    [Inject]
    public MetaObjectFactory(DiContainer container) =>
      _container = container;

    public MetaTile SpawnTile(MetaTileConfig config, Vector3 position)
    {
      if (config == null || config.TilePrefab == null)
      {
        Debug.LogError("[MetaObjectFactory] Cannot spawn tile: missing config or prefab.");
        return null;
      }

      Clear();

      var go = _container.InstantiatePrefab(config.TilePrefab.gameObject, position, Quaternion.identity, null);
      SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());

      _currentInstance = go;
      CurrentTile = go.GetComponent<MetaTile>();

      if (CurrentTile == null)
        Debug.LogError($"[MetaObjectFactory] Spawned prefab '{config.TilePrefab.name}' has no MetaTile component!");

      return CurrentTile;
    }

    public void Clear()
    {
      if (_currentInstance != null)
        Object.Destroy(_currentInstance);

      _currentInstance = null;
      CurrentTile = null;
    }
  }
}