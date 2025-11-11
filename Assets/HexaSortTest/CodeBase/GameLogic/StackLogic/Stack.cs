using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService;

namespace HexaSortTest.CodeBase.GameLogic.StackLogic
{
  public class Stack : MonoBehaviour
  {
    [SerializeField] private List<GameObject> _stack = new();
    private Transform _parent;
    private Transform _defaultParent;
    private ObjectPool<Cell> _poolInstance;

    private const int COLOR_THRESHOLD = 20;

    public IReadOnlyList<GameObject> Tiles => _stack;
    public Transform Parent => _parent;
    public Transform DefaultParent => _defaultParent;

    public void Initialize(ObjectPool<Cell> poolInstance)
    {
      _poolInstance = poolInstance;
    }

    public void SetParent(Transform parent)
    {
      _parent = parent;
      transform.SetParent(parent);

      if (_defaultParent == null)
        _defaultParent = parent;
    }

    public void ResetParent()
    {
      if (_defaultParent != null)
        SetParent(_defaultParent);
    }

    public void Add(GameObject cell)
    {
      if (cell == null) return;
      _stack.Add(cell);
      CheckForColorThreshold();
    }

    public void Remove(GameObject cell)
    {
      if (cell == null) return;
      _stack.Remove(cell);
      CheckForColorThreshold();
    }

    public void SetActive(bool active)
    {
      foreach (var go in _stack)
        if (go != null)
          go.SetActive(active);
    }

    public Color GetLastCellColor()
    {
      if (_stack.Count == 0)
        return Color.clear;

      var lastCell = _stack.Last().GetComponent<Cell>();
      return lastCell != null ? lastCell.Color : Color.clear;
    }

    public void Clear() => _stack.Clear();

    private void CheckForColorThreshold()
    {
      if (_stack.Count < COLOR_THRESHOLD)
        return;
      
      var colorGroups = _stack
        .Where(go => go != null)
        .Select(go => go.GetComponent<Cell>())
        .Where(cell => cell != null)
        .GroupBy(cell => cell.Color)
        .ToList();

      foreach (var group in colorGroups)
      {
        if (group.Count() >= COLOR_THRESHOLD)
        {
          foreach (var cell in group.ToList())
          {
            _stack.Remove(cell.gameObject);
            cell.gameObject.SetActive(false);
            _poolInstance?.ReturnObject(cell);
          }

          Debug.Log($"Removed {group.Count()} tiles of color {group.Key}");
        }
      }

      if (_stack.Count == 0)
      {
        var parent = Parent.GetComponent<Cell>();
        parent?.ShineOff();
        parent?.SetEmpty(true);
        Clear();
        Destroy(gameObject);
      }
    }
  }
}
