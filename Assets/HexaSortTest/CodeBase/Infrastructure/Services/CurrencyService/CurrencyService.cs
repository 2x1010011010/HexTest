using System;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using HexaSortTest.CodeBase.Infrastructure.Services.SaveAndLoadService;

namespace HexaSortTest.CodeBase.Infrastructure.Services.CurrencyService
{
  public class CurrencyService : ICurrencyService
  {
    private readonly IPersistentProgressService _progressService;
    private readonly ISaveLoadService _saveLoadService;

    public event Action<int> OnCoinsChanged;
    public event Action<int> OnHexCoinsChanged;

    public int Coins => _progressService.PlayerProgress.Coins;
    public int HexCoins => _progressService.PlayerProgress.HexCoins;

    public CurrencyService(IPersistentProgressService progressService, ISaveLoadService saveLoadService)
    {
      _progressService = progressService;
      _saveLoadService = saveLoadService;
    }

    public void AddCoins(int amount)
    {
      if (amount <= 0) return;

      _progressService.PlayerProgress.Coins += amount;
      _saveLoadService.SaveProgress();
      OnCoinsChanged?.Invoke(Coins);
    }

    public bool TrySpendCoins(int amount)
    {
      if (amount <= 0) return true;
      if (Coins < amount) return false;

      _progressService.PlayerProgress.Coins -= amount;
      _saveLoadService.SaveProgress();
      OnCoinsChanged?.Invoke(Coins);
      return true;
    }

    public void AddHexCoins(int amount)
    {
      if (amount <= 0) return;

      _progressService.PlayerProgress.HexCoins += amount;
      _saveLoadService.SaveProgress();
      OnHexCoinsChanged?.Invoke(HexCoins);
    }

    public bool TrySpendHexCoins(int amount)
    {
      if (amount <= 0) return true;
      if (HexCoins < amount) return false;

      _progressService.PlayerProgress.HexCoins -= amount;
      _saveLoadService.SaveProgress();
      OnHexCoinsChanged?.Invoke(HexCoins);
      return true;
    }
  }
}