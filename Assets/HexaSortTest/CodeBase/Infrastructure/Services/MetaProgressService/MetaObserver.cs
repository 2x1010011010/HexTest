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
      if (tile == null || tile.IsTileFullyOpen || tile.GroupsCount == 0 ||
          string.IsNullOrWhiteSpace(tile.TileId) || tile.HexCoinsCostPerStep < 0 ||
          _currencyService.HexCoins < tile.HexCoinsCostPerStep)
        return false;

      var before = new MetaTileProgress(tile.TileId)
      {
        UnlockedGroupsCount = tile.CurrentGroupIndex,
        UnlockedObjectsInGroupCount = tile.CurrentObjectIndexInGroup,
        CurrentObjectProgress = tile.CurrentObjectProgress
      };
      MetaTileStepResult result = MetaTileStepResult.None;
      try
      {
        _saveLoadService.ExecuteTransaction(() =>
        {
          result = tile.AdvanceStep(notify: false);
          if (!result.Success) return;
          if (!_currencyService.TrySpendHexCoins(tile.HexCoinsCostPerStep))
            throw new System.InvalidOperationException("Meta balance changed during a synchronous step.");
          SaveProgress(tile);
        });
      }
      catch
      {
        tile.InitializeFromSave(before);
        throw;
      }

      // UI unlock checks now see the just-saved progress, including the last group.
      tile.NotifyStepCompleted(result);
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