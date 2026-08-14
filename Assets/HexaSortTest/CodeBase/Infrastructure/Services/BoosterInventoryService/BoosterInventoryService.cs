using System;
using System.Collections.Generic;
using HexaSortTest.CodeBase.GameLogic.Boosters;
using HexaSortTest.CodeBase.GameLogic.Data;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using HexaSortTest.CodeBase.Infrastructure.Services.SaveAndLoadService;

namespace HexaSortTest.CodeBase.Infrastructure.Services.BoosterInventoryService
{
  public class BoosterInventoryService : IBoosterInventoryService
  {
    private readonly IPersistentProgressService _progressService;
    private readonly ISaveLoadService _saveLoadService;

    public event Action<BoosterType, int> OnCountChanged;

    public BoosterInventoryService(IPersistentProgressService progressService, ISaveLoadService saveLoadService)
    {
      _progressService = progressService;
      _saveLoadService = saveLoadService;
    }

    public int GetCount(BoosterType type) =>
      FindEntry(type)?.Count ?? 0;

    public void Add(BoosterType type, int amount)
    {
      if (amount <= 0) return;

      var entry = GetOrCreateEntry(type);
      entry.Count += amount;

      _saveLoadService.SaveProgress();
      OnCountChanged?.Invoke(type, entry.Count);
    }

    public bool TrySpend(BoosterType type)
    {
      var entry = FindEntry(type);
      if (entry == null || entry.Count <= 0)
        return false;

      entry.Count--;

      _saveLoadService.SaveProgress();
      OnCountChanged?.Invoke(type, entry.Count);
      return true;
    }

    private BoosterInventoryEntry FindEntry(BoosterType type) =>
      _progressService.PlayerProgress.Boosters?.Find(b => b.Type == type);

    private BoosterInventoryEntry GetOrCreateEntry(BoosterType type)
    {
      var progress = _progressService.PlayerProgress;

      if (progress.Boosters == null)
        progress.Boosters = new List<BoosterInventoryEntry>();

      var entry = progress.Boosters.Find(b => b.Type == type);
      if (entry == null)
      {
        entry = new BoosterInventoryEntry(type, 0);
        progress.Boosters.Add(entry);
      }

      return entry;
    }
  }
}