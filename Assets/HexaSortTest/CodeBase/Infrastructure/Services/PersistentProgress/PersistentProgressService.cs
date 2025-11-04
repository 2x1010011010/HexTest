using HexaSortTest.CodeBase.GameLogic.Data;

namespace HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress
{
  public class PersistentProgressService : IPersistentProgressService
  {
    public PlayerProgress PlayerProgress { get; set; }
  }
}