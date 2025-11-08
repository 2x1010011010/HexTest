using System.Collections.Generic;
using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.Spawners;
using HexaSortTest.CodeBase.Infrastructure.Services.AssetManagement;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.Factories
{
  public class GameFactory : IGameFactory
  {
    private readonly IAssetProvider _assets;
    private readonly LevelConfigsList _levelConfigs;
    private LevelConfig _currentLevelConfig;

    public List<IProgressReader> ProgressReaders { get; } = new List<IProgressReader>();

    public List<IProgressSaver> ProgressSavers { get; } = new List<IProgressSaver>();


    public GameFactory(IAssetProvider assets)
    {
      _assets = assets;
      _levelConfigs = Resources.Load<LevelConfigsList>(AssetPaths.LevelConfigs);
    }

    public GridSpawner CreateGridSpawner()
    {
      var gridSpawnerObject = InstantiateRegistered(AssetPaths.GridSpawner);
      var gridSpawner = gridSpawnerObject.GetComponent<GridSpawner>();
      var configIndex = Random.Range(0, _levelConfigs.Levels.Count);
      _currentLevelConfig = _levelConfigs.Levels[configIndex];
      gridSpawner.Initialize(_currentLevelConfig.GridPrefab);
      return gridSpawner;
    }

    public void CreateStacsSpawner()
    {
      var stacksSpawnerObject = InstantiateRegistered(AssetPaths.StackSpawner);
      stacksSpawnerObject.GetComponent<StacksSpawner>().Initialize(_currentLevelConfig,_assets.Instantiate(AssetPaths.CellPrefab), _assets.Instantiate(AssetPaths.StackPrefab));
    }

    public void CreateHud() => 
      InstantiateRegistered(AssetPaths.HUD);
    

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
      ProgressReaders.Clear();
      ProgressSavers.Clear();
    }
  }
}