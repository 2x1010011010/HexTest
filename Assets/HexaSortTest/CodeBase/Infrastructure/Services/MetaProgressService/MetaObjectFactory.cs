using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.Meta;
using HexaSortTest.CodeBase.Infrastructure.Logging;
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
#if UNITY_EDITOR
        GameLogger.Error("[MetaObjectFactory] Cannot spawn tile: missing config or prefab.");
#endif
        return null;
      }

      Clear();
#if UNITY_EDITOR
      GameLogger.Message($"[MetaObjectFactory] Spawning {config.TilePrefab.name} at {position}.");
#endif

      var go = _container.InstantiatePrefab(config.TilePrefab.gameObject, position, Quaternion.identity, null);
      SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());

      _currentInstance = go;
      CurrentTile = go.GetComponent<MetaTile>();

      if (CurrentTile == null)
      {
#if UNITY_EDITOR
        GameLogger.Error($"[MetaObjectFactory] Spawned prefab '{config.TilePrefab.name}' has no MetaTile component!");
#endif
      }

      return CurrentTile;
    }

    public void Clear()
    {
#if UNITY_EDITOR
      GameLogger.Message("[MetaObjectFactory] Clearing current meta tile.");
#endif
      if (_currentInstance != null)
        Object.Destroy(_currentInstance);

      _currentInstance = null;
      CurrentTile = null;
    }
  }
}