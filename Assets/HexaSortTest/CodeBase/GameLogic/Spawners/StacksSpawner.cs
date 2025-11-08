using System.Collections.Generic;
using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Spawners
{
  public class StacksSpawner : GameObjectPool
  {
    [SerializeField, BoxGroup("POINTS")] private List<Transform> _spawnPoints;

    [SerializeField, BoxGroup("POOL PARAMETERS")] private int _maxTilesInPool = 250;

    [SerializeField, BoxGroup("SPAWNER PARAMETERS")] private int _minTilesToSpawn = 2;
    [SerializeField, BoxGroup("SPAWNER PARAMETERS")] private int _maxTilesToSpawn = 10;
    [SerializeField, BoxGroup("SPAWNER PARAMETERS")] private int _maxColorsInStack = 3;
    [SerializeField, BoxGroup("SPAWNER PARAMETERS")] private float _verticalShift = 0.5f;

    private LevelConfig _levelConfig;
    private GameObject _tilePrefab;
    private GameObject _stack;
    private List<GameObject> _spawnedStacks = new();

    public void Initialize(LevelConfig levelConfig, GameObject tilePrefab, GameObject stack)
    {
      _levelConfig = levelConfig;
      _tilePrefab = tilePrefab;
      _stack = stack;
      SetPool(_tilePrefab, this.transform, _maxColorsInStack);

      PrepareStack();
      Spawn();
    }

    public void Spawn()
    {
      for (int i=0; i < _spawnPoints.Count; i++)
      {
        _spawnedStacks[i].GetComponent<Stack>().SetActive(true);
      }
    }

    private void PrepareStack()
    {
      for (int i = 0; i < _spawnPoints.Count; i++)
      {
        var stack = GenerateStack(_spawnPoints[i]);
        _spawnedStacks.Add(stack);
      }
    }

    private GameObject GenerateStack(Transform parent)
    {
      GameObject stackObject = Instantiate(_stack, parent.position, Quaternion.identity, parent);
      stackObject.name = $"Stack {parent.GetSiblingIndex()}";

      Stack stack = stackObject.GetComponent<Stack>();
      stack.SetParent(parent);

      int tilesCount = Random.Range(_minTilesToSpawn, _maxTilesToSpawn);

      for (int i = 0; i < tilesCount; i++)
      {
        if (TryGetObject(out GameObject tile))
        {
          tile.transform.SetParent(stackObject.transform);
          tile.transform.localPosition = Vector3.zero + Vector3.up * _verticalShift;
          stack.Add(tile);
        }
      }
      
      return stackObject;
    }
  }
}