using HexaSortTest.CodeBase.GameLogic.Data;

namespace HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress
{
  public interface IProgressReader
  {
    void LoadProgress(PlayerProgress progress);
  }

  public interface IProgressSaver : IProgressReader
  {
    void UpdateProgress(PlayerProgress progress);
  }
}