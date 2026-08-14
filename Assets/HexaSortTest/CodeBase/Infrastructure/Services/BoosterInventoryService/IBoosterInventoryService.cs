using System;
using HexaSortTest.CodeBase.GameLogic.Boosters;

namespace HexaSortTest.CodeBase.Infrastructure.Services.BoosterInventoryService
{
  public interface IBoosterInventoryService : IService
  {
    int GetCount(BoosterType type);
    void Add(BoosterType type, int amount);
    bool TrySpend(BoosterType type);

    event Action<BoosterType, int> OnCountChanged;
  }
}