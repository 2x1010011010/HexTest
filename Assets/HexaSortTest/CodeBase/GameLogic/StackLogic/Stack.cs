using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;
using HexaSortTest.CodeBase.GameLogic.UI.HUD;
using HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService;
using Sirenix.OdinInspector;

namespace HexaSortTest.CodeBase.GameLogic.StackLogic
{
  public class Stack : MonoBehaviour
  {
    [SerializeField, BoxGroup("STACK SETUP")] private List<GameObject> _stack = new();
    
    [SerializeField, BoxGroup("ANIMATION SETTINGS")] private float _pauseBetween = 0.05f;
    [SerializeField, BoxGroup("ANIMATION SETTINGS")] private float _scaleDuration = 0.2f;
    
    private Transform _parent;
    private Transform _defaultParent;
    private ObjectPool<StackTile> _poolInstance;
    private Cell _parentCell;
    private bool _isDragged;
    private StackAnimator _stackAnimator;

    private const int COLOR_THRESHOLD = 20;

    public List<GameObject> Tiles => _stack;
    public List<StackTile> Cells => _stack.Select(go => go.GetComponent<StackTile>()).ToList();
    public Transform Parent => _parent;
    public Transform DefaultParent => _defaultParent;
    public Cell Cell => _parentCell;
    public bool IsDragged => _isDragged;
    public ObjectPool<StackTile> PoolInstance => _poolInstance;

    public void Initialize(ObjectPool<StackTile> poolInstance)
    {
      _poolInstance = poolInstance;
      _stackAnimator = GetComponent<StackAnimator>();
    }

    public void SetParentCell(Transform parent)
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
        SetParentCell(_defaultParent);
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

      List<StackTile> colorGroups = new();
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

      Debug.Log($"Removed {colorGroups.Count} tiles of color {color}");
      await _stackAnimator.DestroyTilesAnimation(colorGroups, this, 1);
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
    
    public async Task BreakStackByHammer(int tilesAddToCounter = 1)
    {
      if (_stack.Count == 0)
      {
        CheckForEmptyStack();
        return;
      }
      
      var tiles = Cells.Where(c => c != null).Reverse().ToList();

      await _stackAnimator.DestroyTilesAnimation(tiles, this, tilesAddToCounter);

      CheckForEmptyStack();
    }
  }
}
