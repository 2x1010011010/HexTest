using System.Collections.Generic;
using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using HexaSortTest.CodeBase.Infrastructure.Services.AssetManagement;
using HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Spawners
{
  public class StacksSpawner : MonoBehaviour
  {
    [SerializeField, BoxGroup("POINTS")] private List<Transform> _spawnPoints;

    [SerializeField, BoxGroup("POOL PARAMETERS")] private int _maxTilesInPool = 250;

    [SerializeField, BoxGroup("SPAWNER PARAMETERS")] private int _minTilesToSpawn = 5;
    [SerializeField, BoxGroup("SPAWNER PARAMETERS")] private int _maxTilesToSpawn = 10;
    [SerializeField, BoxGroup("SPAWNER PARAMETERS")] private int _maxColorsInStack = 3;
    [SerializeField, BoxGroup("SPAWNER PARAMETERS")] private float _verticalShift = 0.5f;

    private LevelConfig _levelConfig;
    private GameObject _tilePrefab;
    private GameObject _stack;
    private ObjectPool<Cell> _poolInstance;
    private List<GameObject> _spawnedStacks = new();
    private bool _isSpawned = false;
    private float _spawnTimer = 0f;

    public void Initialize(LevelConfig levelConfig, ObjectPool<Cell> poolInstance)
    {
      _levelConfig = levelConfig;
      _tilePrefab = Resources.Load<GameObject>(AssetPaths.CellPrefab);
      _stack = Resources.Load<GameObject>(AssetPaths.StackPrefab);
      _poolInstance = poolInstance;
      PrepareStack();

    }

    private void Update()
    {
      while(_spawnTimer < 25)
        _spawnTimer += Time.deltaTime;
      if (_spawnTimer >= 25)
      {
        Spawn();
        _spawnTimer = 0f;
      }
    }

    public void Spawn()
    {
      if (_isSpawned) return;
      _isSpawned = true;
      for (int i = 0; i < _spawnPoints.Count; i++)
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
        if (!_poolInstance.TryGetObject(out Cell tile)) return null;
        if (tile == null) continue;
        
        tile.SetParent(stack.transform);
        tile.transform.position = stack.transform.position + Vector3.up * (_verticalShift * i);
        stack.Add(tile.gameObject);
      }

      return stackObject;
    }
  }
}