using System.Collections.Generic;
using HexaSortTest.CodeBase.GameLogic.Data;
using HexaSortTest.CodeBase.GameLogic.Meta;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using HexaSortTest.CodeBase.Infrastructure.Services.SaveAndLoadService;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.MetaProgressService
{
  public class MetaObserver : IMetaObserver
  {
    private readonly IPersistentProgressService _progressService;
    private readonly ISaveLoadService _saveLoadService;

    public MetaObserver(IPersistentProgressService progressService, ISaveLoadService saveLoadService)
    {
      _progressService = progressService;
      _saveLoadService = saveLoadService;
    }

    public bool OpenTile(MetaTile tile)
    {
      if (tile == null || tile.IsTileOpen)
        return false;

      var progress = _progressService.PlayerProgress;

      if (progress.HexCoins < tile.CurrencyCostPerTap)
      {
        Debug.Log($"[MetaObserver] Not enough HexCoins to progress tile '{tile.TileId}'. " +
                  $"Have {progress.HexCoins}, need {tile.CurrencyCostPerTap}.");
        return false;
      }

      progress.HexCoins -= tile.CurrencyCostPerTap;
      tile.Open();

      SaveData(tile);
      return true;
    }

    private void SaveData(MetaTile tile)
    {
      var progress = _progressService.PlayerProgress;

      if (progress.MetaProgress == null)
        progress.MetaProgress = new List<MetaTileProgress>();

      var entry = progress.MetaProgress.Find(p => p.TileId == tile.TileId);
      if (entry == null)
      {
        entry = new MetaTileProgress(tile.TileId);
        progress.MetaProgress.Add(entry);
      }

      entry.UnlockedObjectsCount = tile.CurrentObjectIndex;
      entry.CurrentObjectProgress = tile.CurrentObjectProgress;

      _saveLoadService.SaveProgress();
    }
  }
}