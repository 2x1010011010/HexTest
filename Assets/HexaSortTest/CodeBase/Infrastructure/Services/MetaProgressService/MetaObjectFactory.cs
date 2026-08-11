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

    public MetaTile SpawnTile(MetaTileConfig config, Transform spawnPoint)
    {
      if (config == null || config.TilePrefab == null || spawnPoint == null)
      {
        Debug.LogError("[MetaObjectFactory] Cannot spawn tile: missing config, prefab, or spawn point.");
        return null;
      }
      
      Clear();

      var go = _container.InstantiatePrefab(
        config.TilePrefab.gameObject,
        spawnPoint.position,
        spawnPoint.rotation,
        null);

      SceneManager.MoveGameObjectToScene(go, spawnPoint.gameObject.scene);

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