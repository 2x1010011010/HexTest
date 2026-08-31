using System.Collections.Generic;
using HexaSortTest.CodeBase.GameLogic.Data;
using HexaSortTest.CodeBase.GameLogic.Meta;
using HexaSortTest.CodeBase.Infrastructure.Services.CurrencyService;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using HexaSortTest.CodeBase.Infrastructure.Services.SaveAndLoadService;

namespace HexaSortTest.CodeBase.Infrastructure.Services.MetaProgressService
{
  public class MetaObserver : IMetaObserver
  {
    private readonly IPersistentProgressService _progressService;
    private readonly ISaveLoadService _saveLoadService;
    private readonly ICurrencyService _currencyService;

    public MetaObserver(
      IPersistentProgressService progressService,
      ISaveLoadService saveLoadService,
      ICurrencyService currencyService)
    {
      _progressService = progressService;
      _saveLoadService = saveLoadService;
      _currencyService = currencyService;
    }

    public bool TryProgressTile(MetaTile tile)
    {
      if (tile == null || tile.IsTileFullyOpen)
        return false;

      if (!_currencyService.TrySpendHexCoins(tile.HexCoinsCostPerStep))
        return false;

      var result = tile.AdvanceStep();

      if (result.ObjectRevealed)
        SaveProgress(tile);

      return result.Success;
    }

    private void SaveProgress(MetaTile tile)
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

      entry.UnlockedGroupsCount = tile.CurrentGroupIndex;
      entry.UnlockedObjectsInGroupCount = tile.CurrentObjectIndexInGroup;
      entry.CurrentObjectProgress = tile.CurrentObjectProgress;

      _saveLoadService.SaveProgress();
    }
  }
}