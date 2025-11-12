using System.Collections.Generic;
using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using HexaSortTest.CodeBase.GameLogic.Spawners;
using HexaSortTest.CodeBase.Infrastructure.Services.AssetManagement;
using HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.Factories
{
  public class GameFactory : IGameFactory
  {
    private readonly IAssetProvider _assets;
    private readonly LevelConfigsList _levelConfigs;
    private LevelConfig _currentLevelConfig;
    
    private List<GameObject> _instances = new List<GameObject>();

    public List<IProgressReader> ProgressReaders { get; } = new List<IProgressReader>();

    public List<IProgressSaver> ProgressSavers { get; } = new List<IProgressSaver>();


    public GameFactory(IAssetProvider assets)
    {
      _assets = assets;
      _levelConfigs = Resources.Load<LevelConfigsList>(AssetPaths.LevelConfigs);
    }

    public ObjectPool<Cell> CreateCellPool()
    {
      Transform container = new GameObject("PoolContainer").transform;
      var cellPoolInstance = new ObjectPool<Cell>(container);
      for (int i = 0; i < 250; i++)
      {
        var cellPrefab = _assets.Instantiate(AssetPaths.CellPrefab);
        cellPoolInstance.AddToPool(cellPrefab.GetComponent<Cell>());
      }
      
      _instances.Add(container.gameObject);

      return cellPoolInstance;
    }

    public GridSpawner CreateGridSpawner(ObjectPool<Cell> poolInstance)
    {
      var gridSpawnerObject = InstantiateRegistered(AssetPaths.GridSpawner);
      var gridSpawner = gridSpawnerObject.GetComponent<GridSpawner>();
      var configIndex = Random.Range(0, _levelConfigs.Levels.Count);
      _currentLevelConfig = _levelConfigs.Levels[configIndex];
      gridSpawner.Initialize(_currentLevelConfig.GridPrefab);
      
      _instances.Add(gridSpawnerObject);
      
      return gridSpawner;
    }

    public void CreateStacksSpawner(ObjectPool<Cell> poolInstance, HexGrid grid)
    {
      var stacksSpawnerObject = InstantiateRegistered(AssetPaths.StackSpawner);
      stacksSpawnerObject.GetComponent<StacksSpawner>().Initialize(_currentLevelConfig, poolInstance, grid);
      _instances.Add(stacksSpawnerObject);
    }

    public void CreateHud()
    {
      var instance = InstantiateRegistered(AssetPaths.HUD);
      _instances.Add(instance);
    }

    public void CreateMainMenu()
    {
      var instance = InstantiateRegistered(AssetPaths.MainMenuPath);
      _instances.Add(instance);
    }


    private GameObject InstantiateRegistered(string path)
    {
      GameObject prefab = _assets.Instantiate(path);
      RegisterPlayerProgress(prefab);
      return prefab;
    }
    
    private GameObject InstantiateRegistered(string path, Transform at)
    {
      GameObject prefab = _assets.Instantiate(path, at);
      RegisterPlayerProgress(prefab);
      return prefab;
    }

    private void RegisterPlayerProgress(GameObject player)
    {
      foreach (IProgressReader reader in player.GetComponentsInChildren<IProgressReader>())
        Register(reader);
    }

    private void Register(IProgressReader reader)
    {
      if (reader is IProgressSaver saver)
        ProgressSavers.Add(saver);
      
      ProgressReaders.Add(reader);
    }

    public void Clear()
    {
      foreach (GameObject instance in _instances)
      {
        Object.Destroy(instance);
      }
      _instances.Clear();
      ProgressReaders.Clear();
      ProgressSavers.Clear();
    }
  }
}