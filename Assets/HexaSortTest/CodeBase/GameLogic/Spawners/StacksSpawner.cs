using System.Collections.Generic;
using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Spawners
{
  public class StacksSpawner : MonoBehaviour
  {
    [SerializeField, BoxGroup("SETUP")] private List<Transform> _spawnPoints;
    private LevelConfig _levelConfig;
    private ObjectPool<Cell> _cellPool;
    
    public void Initialize(LevelConfig levelConfig)
    {
      _levelConfig = levelConfig;
    }

    public void Spawn()
    {
    }
  }
}