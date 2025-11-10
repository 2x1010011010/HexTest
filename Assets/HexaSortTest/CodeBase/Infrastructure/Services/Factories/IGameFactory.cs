using System.Collections.Generic;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using HexaSortTest.CodeBase.GameLogic.Spawners;
using HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.Factories
{
  public interface IGameFactory : IFactory
  {
    public ObjectPool<Cell> CreateCellPool();
    public GridSpawner CreateGridSpawner(ObjectPool<Cell> pool);
    void CreateStacksSpawner(ObjectPool<Cell> pool, HexGrid grid);
    void CreateHud();
    List<IProgressReader> ProgressReaders { get; }
    List<IProgressSaver> ProgressSavers { get; }
    void Clear();
  }
}