using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;
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
    [SerializeField, BoxGroup("SPAWNER PARAMETERS")] private int _minTilesToSpawn = 3;
    [SerializeField, BoxGroup("SPAWNER PARAMETERS")] private int _maxTilesToSpawn = 10;
    [SerializeField, BoxGroup("SPAWNER PARAMETERS")] private float _verticalShift = 0.7f;
    private LevelConfig _levelConfig;
    private GameObject _stackPrefab;
    private GameObject _tilePrefab;
    private ObjectPool<StackTile> _pool;
    private readonly List<Stack> _tray = new();
    public List<Transform> SpawnPoints => _spawnPoints;
    public bool HasTrayStacks => _tray.Count > 0;
    public bool IsTrayStack(Stack stack) => stack != null && _tray.Contains(stack);

    public void Initialize(LevelConfig config, ObjectPool<StackTile> pool, HexGrid grid)
    {
      _levelConfig = config;
      _pool = pool;
      _stackPrefab = Resources.Load<GameObject>(AssetPaths.StackPrefab);
      _tilePrefab = Resources.Load<GameObject>(AssetPaths.StackTile);
      foreach (var cell in grid.Cells)
      {
        if (!cell.IsSpawner) continue;
        var stack = GenerateStack(cell.transform);
        stack.transform.position += Vector3.up * 0.5f;
        cell.SetEmpty(false);
        cell.SetSpawner(false);
      }
      Refill();
    }

    private void Refill()
    {
      foreach (var point in _spawnPoints)
      {
        var stack = GenerateStack(point);
        stack.GetComponent<StackMover>().OnStackParentChange += StackParentChanged;
        _tray.Add(stack);
      }
      AudioFacade.Instance?.PlaySpawn();
    }

    private Stack GenerateStack(Transform parent)
    {
      var go = Instantiate(_stackPrefab, parent.position, Quaternion.identity);
      var stack = go.GetComponent<Stack>();
      stack.SetParentCell(parent);
      stack.Initialize(_pool);
      int count = Random.Range(_minTilesToSpawn, _maxTilesToSpawn);
      int split = Random.Range(0, count);
      var first = RandomColor();
      var second = RandomColor();
      for (int i = 0; i < count; i++)
      {
        // The prewarmed pool may be exhausted by larger fields; grow it instead of returning a partial/null stack.
        if (!_pool.TryGetObject(out StackTile tile)) tile = Instantiate(_tilePrefab).GetComponent<StackTile>();
        tile.Color = i < split ? first : second;
        tile.SetParent(stack.transform);
        tile.transform.SetPositionAndRotation(stack.transform.position + Vector3.up * (_verticalShift * i), Quaternion.Euler(0, 90, 0));
        tile.SetActive(true);
        stack.Add(tile.gameObject);
      }
      return stack;
    }

    private Color RandomColor() => _levelConfig.CellColors[Random.Range(0, _levelConfig.CellColors.Count)];

    public void StackParentChanged(Stack stack)
    {
      if (!_tray.Remove(stack)) return;
      stack.GetComponent<StackMover>().OnStackParentChange -= StackParentChanged;
      if (_tray.Count == 0) Refill();
    }

    public async UniTask ReplaceTrayAsync(CancellationToken token)
    {
      var snapshot = _tray.ToArray();
      foreach (var stack in snapshot)
      {
        token.ThrowIfCancellationRequested();
        stack.GetComponent<StackMover>().OnStackParentChange -= StackParentChanged;
        await stack.RemoveAllAsync(token);
      }
      _tray.Clear();
      Refill();
    }

    private void OnDestroy()
    {
      foreach (var stack in _tray)
        if (stack != null) stack.GetComponent<StackMover>().OnStackParentChange -= StackParentChanged;
    }
  }
}
