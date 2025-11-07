using System.Collections.Generic;
using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Spawners
{
  public class StacksSpawner : MonoBehaviour
  {
    [SerializeField, BoxGroup("POINTS")] private List<Transform> _spawnPoints;
    [SerializeField, BoxGroup("SPAWNER PARAMETERS")] private int _minTilesToSpawn = 2;
    [SerializeField, BoxGroup("SPAWNER PARAMETERS")] private int _maxTilesToSpawn = 10;
    [SerializeField, BoxGroup("SPAWNER PARAMETERS")] private int _maxColorsInStack = 3;
    
    private LevelConfig _levelConfig;
    private GameObject _tilePrefab;
    private ObjectPool<Cell> _cellPool;
    
    public void Initialize(LevelConfig levelConfig, GameObject tilePrefab)
    {
      _levelConfig = levelConfig;
      _tilePrefab = tilePrefab;
      Spawn();
    }

    public void Spawn()
    {

    }

    private void PrepareStack()
    {

    }
    
    private void SpawnStack(Stack stack, Transform at)
    {
      stack.transform.position = at.localPosition;
    }
  }
}