using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;
using HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService;

namespace HexaSortTest.CodeBase.GameLogic.StackLogic
{
  public class Stack : MonoBehaviour
  {
    [SerializeField] private List<GameObject> _stack = new();
    private Transform _parent;
    private Transform _defaultParent;
    private ObjectPool<Cell> _poolInstance;
    private Cell _parentCell;
    private bool _isDragged;

    private const int COLOR_THRESHOLD = 20;

    public List<GameObject> Tiles => _stack;
    public List<Cell> Cells => _stack.Select(go => go.GetComponent<Cell>()).ToList();
    public Transform Parent => _parent;
    public Transform DefaultParent => _defaultParent;
    public Cell Cell => _parentCell;
    public bool IsDragged => _isDragged;

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

      _parentCell = parent.GetComponent<Cell>();
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
    }

    public void Remove(GameObject cell)
    {
      if (cell == null) return;
      _stack.Remove(cell);
    }

    public void SetActive(bool active)
    {
      foreach (var go in _stack)
        if (go != null)
          go.SetActive(active);
    }
    
    public void SetDragged(bool dragged) => 
      _isDragged = dragged;

    public Color GetLastCellColor()
    {
      if (_stack.Count == 0)
        return Color.clear;

      var lastCell = Cells.Last();
      return lastCell != null ? lastCell.Color : Color.clear;
    }

    public void Clear() => _stack.Clear();

    public async Task CheckForColorThreshold()
    {
      if (_stack.Count < COLOR_THRESHOLD)
      {
        CheckForEmptyStack();
        return;
      }

      List<Cell> colorGroups = new();
      Color color = GetLastCellColor();

      for (int i = _stack.Count - 1; i >= 0; i--)
      {
        if (Cells[i].Color != color) break;
        colorGroups.Add(Cells[i]);
      }

      if (colorGroups.Count < COLOR_THRESHOLD)
      {
        CheckForEmptyStack();
        return;
      }

      float delay = 0f;
      float pauseBetween = 0.2f;
      float scaleDuration = 0.5f;

      foreach (var cell in colorGroups)
      {
        if (cell == null) continue;

        _stack.Remove(cell.gameObject);
        var startScale = cell.transform.localScale;

        cell.transform.DOScale(Vector3.zero, scaleDuration)
          .SetDelay(delay)
          .SetEase(Ease.InOutSine)
          .OnStart(() => AudioFacade.Instance.PlayClose())
          .OnComplete(() =>
          {
            cell.SetActive(false);
            cell.transform.localScale = startScale;
            cell.transform.position = Vector3.zero;
            cell.Color = Color.white;
            _poolInstance?.ReturnObject(cell);
          });

        delay += pauseBetween;
      }

      Debug.Log($"Removed {colorGroups.Count} tiles of color {color}");
      
      float totalAnimationTime = delay + scaleDuration;
      await Task.Delay(Mathf.RoundToInt(totalAnimationTime * 1000f));

      CheckForEmptyStack();
    }

    private void CheckForEmptyStack()
    {
      if (_stack.Count == 0)
      {
        var parent = _parentCell;
        parent?.ShineOff();
        parent?.SetEmpty(true);
        Clear();
        Destroy(gameObject);
      }
    }
  }
}
