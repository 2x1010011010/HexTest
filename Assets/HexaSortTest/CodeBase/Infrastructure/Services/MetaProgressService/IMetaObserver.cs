using HexaSortTest.CodeBase.GameLogic.Meta;

namespace HexaSortTest.CodeBase.Infrastructure.Services.MetaProgressService
{
  public interface IMetaObserver : IService
  {
    bool TryProgressTile(MetaTile tile);
  }
}