using System;

namespace HexaSortTest.CodeBase.Infrastructure.Services.CurrencyService
{
  public interface ICurrencyService : IService
  {
    int Coins { get; }
    int HexCoins { get; }

    event Action<int> OnCoinsChanged;
    event Action<int> OnHexCoinsChanged;

    void AddCoins(int amount);
    bool TrySpendCoins(int amount);

    void AddHexCoins(int amount);
    bool TrySpendHexCoins(int amount);
  }
}