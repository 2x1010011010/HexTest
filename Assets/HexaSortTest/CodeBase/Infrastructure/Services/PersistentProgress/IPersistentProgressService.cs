using HexaSortTest.CodeBase.GameLogic.Data;

namespace HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress
{
  public interface IPersistentProgressService : IService
  {
    PlayerProgress PlayerProgress { get; set; }
  }
}