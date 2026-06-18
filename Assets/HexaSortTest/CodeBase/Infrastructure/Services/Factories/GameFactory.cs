using System.Collections.Generic;
using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using HexaSortTest.CodeBase.GameLogic.Spawners;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using HexaSortTest.CodeBase.GameLogic.UI.HUD;
using HexaSortTest.CodeBase.GameLogic.UI.MainMenu;
using HexaSortTest.CodeBase.Infrastructure.Services.AssetManagement;
using HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;
using Zenject;

namespace HexaSortTest.CodeBase.Infrastructure.Services.Factories
{
  public class GameFactory : IGameFactory
  {
    private readonly DiContainer _container;
    private readonly IAssetProvider _assets;
    private readonly LevelConfigsList _levelConfigs;

    private LevelConfig _currentLevelConfig;
    private StacksSpawner _stacksSpawner;
    private GridSpawner _gridSpawner;
    private readonly List<GameObject> _instances = new();


    public List<IProgressReader> ProgressReaders { get; } = new();
    public List<IProgressSaver> ProgressSavers { get; } = new();

    [Inject]
    public GameFactory(DiContainer container, IAssetProvider assets)
    {
      _container = container;
      _assets = assets;
      _levelConfigs = Resources.Load<LevelConfigsList>(AssetPaths.LevelConfigs);
    }

    public ObjectPool<StackTile> CreateCellPool()
    {
      var container = new GameObject("PoolContainer").transform;
      var pool = new ObjectPool<StackTile>(container);

      for (int i = 0; i < 250; i++)
      {
        var prefab = _assets.Instantiate(AssetPaths.StackTile);
        pool.AddToPool(prefab.GetComponent<StackTile>());
      }

      _instances.Add(container.gameObject);
      return pool;
    }

    public GridSpawner CreateGridSpawner(ObjectPool<StackTile> pool, MainMenuObserver mainMenu)
    {
      var go = InstantiateInjected(AssetPaths.GridSpawner);
      _gridSpawner = go.GetComponent<GridSpawner>();

      _currentLevelConfig = _levelConfigs.Levels[Random.Range(0, _levelConfigs.Levels.Count)];
      _gridSpawner.Initialize(_currentLevelConfig.GridPrefab);
      _gridSpawner.SetMainMenu(mainMenu);

      _instances.Add(go);
      return _gridSpawner;
    }

    public void CreateStacksSpawner(ObjectPool<StackTile> pool, HexGrid grid)
    {
      var go = InstantiateInjected(AssetPaths.StackSpawner);
      _stacksSpawner = go.GetComponent<StacksSpawner>();
      _stacksSpawner.Initialize(_currentLevelConfig, pool, grid);
      _instances.Add(go);
    }

    public void CreateHud(MainMenuObserver mainMenu)
    {
      var go = InstantiateInjected(AssetPaths.HUD);
      go.GetComponent<HudObserver>().Init(_currentLevelConfig.WinCondition, mainMenu, _stacksSpawner);
      _instances.Add(go);
    }

    public MainMenuObserver CreateMainMenu()
    {
      var go = InstantiateInjected(AssetPaths.MainMenuPath);
      _instances.Add(go);
      return go.GetComponent<MainMenuObserver>();
    }

    public void Clear()
    {
      ProgressReaders.Clear();
      ProgressSavers.Clear();

      foreach (var go in _instances)
        if (go != null)
          Object.Destroy(go);

      _instances.Clear();
    }

    private GameObject InstantiateInjected(string resourcePath)
    {
      var prefab = Resources.Load<GameObject>(resourcePath);
      var go = _container.InstantiatePrefab(prefab);
      RegisterPlayerProgress(go);
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