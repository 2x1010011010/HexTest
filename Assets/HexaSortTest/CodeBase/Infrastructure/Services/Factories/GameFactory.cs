using System.Collections.Generic;
using CodeBase.Infrastructure.Services.AssetManagement;
using CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.Infrastructure.Services.AssetManagement;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.Factories
{
  public class GameFactory : IGameFactory
  {
    private readonly IAssetProvider _assets;
    
    public List<IProgressReader> ProgressReaders { get; } = new List<IProgressReader>();
    public List<IProgressSaver> ProgressSavers { get; } = new List<IProgressSaver>();
    
    public GameFactory(IAssetProvider assets)
    {
      _assets = assets;
    }
    
    public GameObject CreatePlayer(Transform spawnPoint) =>
      InstantiateRegistered(AssetPaths.CellPrefab, spawnPoint);
      

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