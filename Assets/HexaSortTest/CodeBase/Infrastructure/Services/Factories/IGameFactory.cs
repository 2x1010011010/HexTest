using System.Collections.Generic;
using HexaSortTest.CodeBase.GameLogic.Spawners;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.Factories
{
  public interface IGameFactory : IFactory
  {
    public GridSpawner CreateGridSpawner();
    void CreateStacsSpawner();
    List<IProgressReader> ProgressReaders { get; }
    List<IProgressSaver> ProgressSavers { get; }
    void Clear();
  }
}