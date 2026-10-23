using System.Collections.Generic;
using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using HexaSortTest.CodeBase.GameLogic.Spawners;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using HexaSortTest.CodeBase.GameLogic.UI.MainMenu;
using HexaSortTest.CodeBase.Infrastructure.Logging;
using HexaSortTest.CodeBase.Infrastructure.Services.AssetManagement;
using HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace HexaSortTest.CodeBase.Infrastructure.Services.Factories
{
  public class GameFactory : IGameFactory
  {
    private readonly DiContainer _container;
    private readonly IAssetProvider _assets;
    private readonly LevelConfigsList _levelConfigs;

    private LevelConfig _currentLevelConfig;
    private GridSpawner _gridSpawner;
    private readonly List<GameObject> _instances = new();

    public LevelConfig CurrentLevelConfig => _currentLevelConfig;

    public List<IProgressReader> ProgressReaders { get; } = new();
    public List<IProgressSaver> ProgressSavers { get; } = new();

    [Inject]
    public GameFactory(DiContainer container, IAssetProvider assets)
    {
      _container = container;
      _assets = assets;
      _levelConfigs = Resources.Load<LevelConfigsList>(AssetPaths.LevelConfigs);
#if UNITY_EDITOR
      if (_levelConfigs == null)
        GameLogger.Error($"[GameFactory] Level configs missing at Resources/{AssetPaths.LevelConfigs}.");
      else
        GameLogger.Message("[GameFactory] Level configs loaded.");
#endif
    }

    public ObjectPool<StackTile> CreateCellPool()
    {
#if UNITY_EDITOR
      GameLogger.Message("[GameFactory] Creating tile pool (250 tiles).");
#endif
      var container = new GameObject("PoolContainer").transform;
      var pool = new ObjectPool<StackTile>(container);

      for (int i = 0; i < 250; i++)
      {
        var prefab = _assets.Instantiate(AssetPaths.StackTile);
        pool.AddToPool(prefab.GetComponent<StackTile>());
      }

      _instances.Add(container.gameObject);
#if UNITY_EDITOR
      GameLogger.Message("[GameFactory] Tile pool ready (250 tiles).");
#endif
      return pool;
    }

    public GridSpawner CreateGridSpawner(ObjectPool<StackTile> pool, MainMenuObserver mainMenu, int levelIndex)
    {
      var go = InstantiateInjected(AssetPaths.GridSpawner);
      _gridSpawner = go.GetComponent<GridSpawner>();

      _currentLevelConfig = _levelConfigs.GetLevel(levelIndex);
#if UNITY_EDITOR
      GameLogger.Message($"[GameFactory] Selected level config for index {levelIndex}.");
#endif
      _gridSpawner.Initialize(_currentLevelConfig.GridPrefab);
      _gridSpawner.SetMainMenu(mainMenu);

      _instances.Add(go);
      return _gridSpawner;
    }

    public StacksSpawner CreateStacksSpawner(ObjectPool<StackTile> pool, HexGrid grid)
    {
      var go = InstantiateInjected(AssetPaths.StackSpawner);
      var stacksSpawner = go.GetComponent<StacksSpawner>();
      stacksSpawner.Initialize(_currentLevelConfig, pool, grid);
      _instances.Add(go);
      return stacksSpawner;
    }

    public void Clear()
    {
#if UNITY_EDITOR
      GameLogger.Message($"[GameFactory] Clearing {_instances.Count} tracked objects and progress registrations.");
#endif
      ProgressReaders.Clear();
      ProgressSavers.Clear();

      foreach (var go in _instances)
        if (go != null)
          Object.Destroy(go);

      _instances.Clear();
    }

    private GameObject InstantiateInjected(string resourcePath)
    {
#if UNITY_EDITOR
      GameLogger.Message($"[GameFactory] Instantiating Resources/{resourcePath} with DI.");
#endif
      var prefab = Resources.Load<GameObject>(resourcePath);
      var go = _container.InstantiatePrefab(prefab);
      go.transform.SetParent(null);
      SceneManager.MoveGameObjectToScene(go, SceneManager.GetActiveScene());
      RegisterPlayerProgress(go);
#if UNITY_EDITOR
      GameLogger.Message($"[GameFactory] Created {go.name} in scene {go.scene.name}.", go);
#endif
      return go;
    }

    private void RegisterPlayerProgress(GameObject go)
    {
      foreach (var reader in go.GetComponentsInChildren<IProgressReader>())
        Register(reader);
    }

    private void Register(IProgressReader reader)
    {
      if (reader is IProgressSaver saver)
        ProgressSavers.Add(saver);

      ProgressReaders.Add(reader);
    }
  }
}
