using System;
using System.Collections.Generic;
using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.Boosters;
using HexaSortTest.CodeBase.Infrastructure.Services.BoosterInventoryService;
using HexaSortTest.CodeBase.Infrastructure.Services.CurrencyService;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using HexaSortTest.CodeBase.Infrastructure.Services.SaveAndLoadService;
using LifeService = HexaSortTest.CodeBase.Infrastructure.Services.LivesService.LivesService;

namespace HexaSortTest.CodeBase.Infrastructure.Services
{
  // Local prototype only. A production IAP adapter must verify a receipt before fulfillment.
  public sealed class ShopService
  {
    private readonly IPersistentProgressService _progress;
    private readonly ISaveLoadService _save;
    private readonly ICurrencyService _currency;
    private readonly IBoosterInventoryService _boosters;
    private readonly LifeService _lives;
    private bool _busy;
    public event Action Changed;
    public ShopService(IPersistentProgressService progress, ISaveLoadService save,
      ICurrencyService currency, IBoosterInventoryService boosters, LifeService lives)
    { _progress = progress; _save = save; _currency = currency; _boosters = boosters; _lives = lives; }

    public bool IsOwned(StorePositionBundle product) => HasUpgrade(product) &&
      _progress.PlayerProgress?.LivesUpgradePurchased == true;

    public static bool HasUpgrade(StorePositionBundle product) => product?.Entries != null &&
      product.Entries.Exists(e => e?.StorePosition != null && e.StorePosition.Type == StorePositionType.LivesUpgrade);

    public static bool Validate(StorePositionBundle product, out string error)
    {
      error = null;
      if (product == null || string.IsNullOrWhiteSpace(product.Sku) || string.IsNullOrWhiteSpace(product.DisplayName) ||
          product.Entries == null || product.Entries.Count == 0)
      { error = "Product needs an ID, title and rewards."; return false; }
      long coins = 0; var totals = new Dictionary<BoosterType, long>(); int upgrades = 0;
      foreach (var entry in product.Entries)
      {
        var item = entry?.StorePosition;
        if (item == null || entry.Quantity <= 0 || item.Amount <= 0)
        { error = "Invalid reward or quantity."; return false; }
        long amount = (long)item.Amount * entry.Quantity;
        switch (item.Type)
        {
          case StorePositionType.Coins: coins += amount; break;
          case StorePositionType.Booster:
            if (!Enum.IsDefined(typeof(BoosterType), item.BoosterType))
            { error = "Unknown booster."; return false; }
            totals.TryGetValue(item.BoosterType, out long old); totals[item.BoosterType] = old + amount;
            if (totals[item.BoosterType] > int.MaxValue) { error = "Too many boosters."; return false; }
            break;
          case StorePositionType.LivesUpgrade:
            if (amount != 1 || ++upgrades > 1) { error = "Use one lives upgrade."; return false; }
            break;
          default: error = "Unsupported reward type."; return false;
        }
        if (coins > int.MaxValue) { error = "Too many coins."; return false; }
      }
      return true;
    }

    public bool TryPurchaseStub(StorePositionBundle product, bool enabled, out string message)
    {
      message = "Purchases are coming soon.";
      if (!enabled || _busy || _progress.PlayerProgress == null) return false;
      if (!Validate(product, out message)) return false;
      if (IsOwned(product)) { message = "Already purchased."; return false; }
      long coins = _currency.Coins;
      var totals = new Dictionary<BoosterType, long>();
      foreach (var e in product.Entries)
      {
        var item = e.StorePosition; long count = (long)item.Amount * e.Quantity;
        if (item.Type == StorePositionType.Coins) coins += count;
        if (item.Type == StorePositionType.Booster)
        {
          if (!totals.ContainsKey(item.BoosterType)) totals[item.BoosterType] = _boosters.GetCount(item.BoosterType);
          totals[item.BoosterType] += count;
        }
      }
      if (coins > int.MaxValue) { message = "Coin limit reached."; return false; }
      foreach (long count in totals.Values)
        if (count > int.MaxValue) { message = "Booster limit reached."; return false; }
      _busy = true;
      try
      {
        _save.ExecuteTransaction(() =>
        {
          foreach (var e in product.Entries)
          {
            var item = e.StorePosition; int amount = checked(item.Amount * e.Quantity);
            switch (item.Type)
            {
              case StorePositionType.Coins: _currency.AddCoins(amount); break;
              case StorePositionType.Booster: _boosters.Add(item.BoosterType, amount); break;
              case StorePositionType.LivesUpgrade:
                var data = _progress.PlayerProgress;
                data.LivesUpgradePurchased = data.AdRemovalPurchased = data.LivesInitialized = true;
                data.Lives = 10; data.NextLifeUtcTicks = 0;
                break;
            }
          }
        });
        message = "Added to your inventory.";
      }
      catch (Exception)
      { message = "Purchase could not be saved. Please try again."; return false; }
      finally { _busy = false; }
      _lives.NotifyChanged();
      Changed?.Invoke();
      return true;
    }
  }
}
