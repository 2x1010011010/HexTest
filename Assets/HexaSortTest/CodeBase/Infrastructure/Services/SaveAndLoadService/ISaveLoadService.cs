using HexaSortTest.CodeBase.GameLogic.Data;

namespace HexaSortTest.CodeBase.Infrastructure.Services.SaveAndLoadService
{
  public interface ISaveLoadService : IService
  {
    PlayerProgress LoadProgress();
    void SaveProgress();
  }
}