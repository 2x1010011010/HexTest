using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.Meta;
using HexaSortTest.CodeBase.Infrastructure.Services.Factories;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.MetaProgressService
{
  public interface IMetaObjectFactory : IFactory
  {
    MetaTile CurrentTile { get; }
    MetaTile SpawnTile(MetaTileConfig config, Transform spawnPoint);
    MetaTile SpawnTile(MetaTileConfig config, Vector3 spawnPoint);
    void Clear();
  }
}