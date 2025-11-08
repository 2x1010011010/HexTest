using System.Collections.Generic;
using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using HexaSortTest.CodeBase.Infrastructure.Services.AssetManagement;
using HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Spawners
{
  public class StacksSpawner : GameObjectPool
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
    private List<GameObject> _spawnedStacks = new();
    private bool _isSpawned = false;
    private float _spawnTimer = 0f;

    public void Initialize(LevelConfig levelConfig)
    {
      _levelConfig = levelConfig;
      Debug.Log("Level Config:");
      _tilePrefab = Resources.Load<GameObject>(AssetPaths.CellPrefab);
      Debug.Log("_tilePrefab");
      _stack = Resources.Load<GameObject>(AssetPaths.StackPrefab);
      Debug.Log("_stack");
      SetPool(_tilePrefab, this.transform, _maxTilesInPool);
      Debug.Log("SetPool");
      PrepareStack();
      Debug.Log("PrepareStack");
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
        Debug.Log($"PrepareStack: {i}");
        var stack = GenerateStack(_spawnPoints[i]);
        Debug.Log($"PrepareStack: {i} - stack");
        _spawnedStacks.Add(stack);
        Debug.Log($"PrepareStack: {i} - stack added");
      }
    }

    private GameObject GenerateStack(Transform parent)
    {
      GameObject stackObject = Instantiate(_stack, parent.position, Quaternion.identity, parent);
      Debug.Log($"GenerateStack: {parent.GetSiblingIndex()}");
      
      stackObject.name = $"Stack {parent.GetSiblingIndex()}";
      
      Stack stack = stackObject.GetComponent<Stack>();
      stack.SetParent(parent);

      int tilesCount = Random.Range(_minTilesToSpawn, _maxTilesToSpawn);

      Debug.Log($"Count = {tilesCount}");
      
      for (int i = 0; i < tilesCount; i++)
      {
        Debug.Log($"GenerateStack for cycle: {i}");
        var tile = GetObject();
        Debug.Log($"GenerateStack for cycle: {i} - tile");
        if (tile == null) continue;
        Debug.Log($"GenerateStack for cycle: {i} - tile != null");
        tile.transform.SetParent(stack.transform);
        Debug.Log($"GenerateStack for cycle: {i} - tile.transform.SetParent");
        tile.transform.position = stack.transform.position + Vector3.up * (_verticalShift * i);
        Debug.Log($"GenerateStack for cycle: {i} - tile.transform.position");
        stack.Add(tile);
        Debug.Log($"GenerateStack for cycle: {i} - stack.Add");
      }
      Debug.Log("GenerateStack: cycle finished");
      return stackObject;
    }
  }
}