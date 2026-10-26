using System;
using HexaSortTest.CodeBase.GameLogic.Data;

namespace HexaSortTest.CodeBase.Infrastructure.Services.SaveAndLoadService
{
  public interface ISaveLoadService : IService
  {
    PlayerProgress LoadProgress();
    void SaveProgress();
    // Synchronous changes are persisted together; failed changes are rolled back in memory.
    void ExecuteTransaction(Action update);
  }
}