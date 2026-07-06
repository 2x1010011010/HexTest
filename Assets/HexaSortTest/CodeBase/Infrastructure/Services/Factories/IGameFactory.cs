using System.Collections.Generic;
using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using HexaSortTest.CodeBase.GameLogic.Spawners;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using HexaSortTest.CodeBase.GameLogic.UI.MainMenu;
using HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;

namespace HexaSortTest.CodeBase.Infrastructure.Services.Factories
{
  public interface IGameFactory : IFactory
  {
    LevelConfig CurrentLevelConfig { get; }
    ObjectPool<StackTile> CreateCellPool();
    GridSpawner CreateGridSpawner(ObjectPool<StackTile> pool, MainMenuObserver mainMenu, int levelIndex);
    StacksSpawner CreateStacksSpawner(ObjectPool<StackTile> pool, HexGrid grid);
    List<IProgressReader> ProgressReaders { get; }
    List<IProgressSaver> ProgressSavers { get; }
    void Clear();
  }
}