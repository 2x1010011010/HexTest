using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.StackLogic
{
  public class Stack : MonoBehaviour
  {
    [SerializeField, BoxGroup("STACK SETUP")] private List<GameObject> _stack = new();
    [SerializeField, Min(1), BoxGroup("STACK SETUP")] private int _colorTreshold = 10;
    private Transform _parent;
    private Transform _defaultParent;
    private ObjectPool<StackTile> _poolInstance;
    private Cell _parentCell;
    private bool _isDragged;
    private StackAnimator _stackAnimator;
    public List<GameObject> Tiles => _stack;
    public List<StackTile> Cells => _stack.Where(go => go != null).Select(go => go.GetComponent<StackTile>()).ToList();
    public Transform Parent => _parent;
    public Transform DefaultParent => _defaultParent;
    public Cell Cell => _parentCell;
    public bool IsDragged => _isDragged;
    public ObjectPool<StackTile> PoolInstance => _poolInstance;

    public void Initialize(ObjectPool<StackTile> pool)
    {
      _poolInstance = pool;
      _stackAnimator = GetComponent<StackAnimator>();
    }

    public void SetParentCell(Transform parent)
    {
      _parent = parent;
      transform.SetParent(parent, true);
      if (_defaultParent == null) _defaultParent = parent;
      _parentCell = parent != null ? parent.GetComponent<Cell>() : null;
    }
    public void ResetParent() { if (_defaultParent != null) SetParentCell(_defaultParent); }
    public void Add(GameObject tile) { if (tile != null) _stack.Add(tile); }
    public void Remove(GameObject tile) => _stack.Remove(tile);
    public void Clear() => _stack.Clear();
    public void SetDragged(bool value) => _isDragged = value;
    public void SetActive(bool value) { foreach (var tile in Cells) tile.SetActive(value); }
    public Color GetLastCellColor() => _stack.Count == 0 ? Color.clear : _stack[_stack.Count - 1].GetComponent<StackTile>().Color;

    public async UniTask<int> CollectTopAsync(CancellationToken token)
    {
      if (_stack.Count < _colorTreshold) return 0;
      var color = GetLastCellColor();
      var group = new List<StackTile>();
      for (int i = _stack.Count - 1; i >= 0; i--)
      {
        var tile = _stack[i].GetComponent<StackTile>();
        if (tile.Color != color) break;
        group.Add(tile);
      }
      if (group.Count < _colorTreshold) return 0;
      await _stackAnimator.DestroyTilesAsync(group, this, token);
      DestroyIfEmpty();
      return group.Count;
    }

    public async UniTask<int> RemoveAllAsync(CancellationToken token)
    {
      var tiles = Cells;
      tiles.Reverse();
      await _stackAnimator.DestroyTilesAsync(tiles, this, token);
      DestroyIfEmpty();
      return tiles.Count;
    }

    public UniTask AnimateMoveToStack(List<GameObject> tiles, Vector3 direction, CancellationToken token) =>
      _stackAnimator.MoveStackTilesAsync(this, tiles, direction, token);

    public void DestroyIfEmpty()
    {
      if (_stack.Count != 0) return;
      _parentCell?.SetEmpty(true);
      _parentCell?.ShineOff();
      Destroy(gameObject);
    }
  }
}
