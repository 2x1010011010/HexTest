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

      // AdvanceStep moves the cursor. Capture THIS cell's cost before moving to the next.
      int stepCost = tile.HexCoinsCostPerStep;
      var before = tile.CaptureProgress();
      MetaTileStepResult result = MetaTileStepResult.None;
      try
      {
        _saveLoadService.ExecuteTransaction(() =>
        {
          result = tile.AdvanceStep(notify: false);
          if (!result.Success) return;
          if (!_currencyService.TrySpendHexCoins(stepCost))
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

    public MetaCellPurchaseResult TryOpenCell(MetaTile tile, MetaLocationCell cell)
    {
      if (tile == null || cell == null || !tile.Cells.Contains(cell) || !tile.ValidateLayout(out _))
        return MetaCellPurchaseResult.Invalid;
      if (tile.IsCellOpen(cell)) return MetaCellPurchaseResult.AlreadyOpened;
      if (_currencyService.HexCoins < cell.HexCoinsCost) return MetaCellPurchaseResult.InsufficientFunds;
      var before = tile.CaptureProgress();
      var result = MetaTileStepResult.None;
      try
      {
        _saveLoadService.ExecuteTransaction(() =>
        {
          // Mark paid before currency callbacks: reentrant clicks cannot buy twice.
          result = tile.OpenCell(cell, deferVisuals: true);
          if (!result.Success) throw new System.InvalidOperationException("Invalid meta cell.");
          if (!_currencyService.TrySpendHexCoins(cell.HexCoinsCost))
            throw new System.InvalidOperationException("Meta balance changed during purchase.");
          SaveProgress(tile);
        });
      }
      catch
      {
        tile.InitializeFromSave(before);
        throw;
      }
      tile.NotifyStepCompleted(result);
      return MetaCellPurchaseResult.Success;
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

      var snapshot = tile.CaptureProgress();
      entry.UsesCellIds = snapshot.UsesCellIds;
      entry.OpenedCellIds = snapshot.OpenedCellIds;
      entry.UnlockedGroupsCount = tile.CurrentGroupIndex;
      entry.UnlockedObjectsInGroupCount = tile.CurrentObjectIndexInGroup;
      entry.CurrentObjectProgress = tile.CurrentObjectProgress;

      _saveLoadService.SaveProgress();
    }
  }
}