using System.Collections.Generic;
using System.Linq;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.GridLogic
{
  public class GridObserver : MonoBehaviour
  {
    [SerializeField, BoxGroup("SETUP")] private HexGrid _grid;

    private readonly Dictionary<Cell, List<Cell>> _neighbors = new();
    private readonly HashSet<Stack> _stacksOnGrid = new();
    private Cell _lastAddedCell;

    public void Init(HexGrid grid) => _grid = grid;

    private void Start()
    {
      if (_grid == null)
      {
        Debug.LogError("Grid not set on GridObserver!");
        return;
      }

      foreach (var cell in _grid.Cells)
        _neighbors[cell] = GetNeighbors(cell);

      ScanAndRegisterStacks();
    }

    private void Update()
    {
      if (Input.GetMouseButtonUp(0))
      {
        if (RescanForNewStacks(out var newCell))
        {
          _lastAddedCell = newCell;
          ProcessMergesFromCell(_lastAddedCell);
        }
      }
    }

    private void ScanAndRegisterStacks()
    {
      _stacksOnGrid.Clear();

      foreach (var cell in _grid.Cells)
      {
        var stack = cell.GetComponentInChildren<Stack>();
        if (stack != null)
          _stacksOnGrid.Add(stack);
      }
      
      CheckAllStacksForMerges();
    }

    private bool RescanForNewStacks(out Cell newCell)
    {
      foreach (var cell in _grid.Cells)
      {
        var stack = cell.GetComponentInChildren<Stack>();
        if (stack == null) continue;
        if (_stacksOnGrid.Contains(stack)) continue;

        _stacksOnGrid.Add(stack);
        newCell = cell;
        return true;
      }

      newCell = null;
      return false;
    }

    private void CheckAllStacksForMerges()
    {
      bool merged;
      do
      {
        merged = false;
        var stacks = _grid.Cells
          .Where(c => c.GetComponentInChildren<Stack>() != null)
          .ToList();

        foreach (var cell in stacks)
        {
          if (ProcessMergesFromCell(cell, recursiveCheck: false))
            merged = true;
        }
      } while (merged);
    }

    private bool ProcessMergesFromCell(Cell centerCell, bool recursiveCheck = true)
    {
      if (centerCell == null) return false;
      var centerStack = centerCell.GetComponentInChildren<Stack>();
      if (centerStack == null) return false;

      bool mergedAny = false;

      var neighborCells = _neighbors[centerCell]
        .Where(n => n != null && n.GetComponentInChildren<Stack>() != null)
        .ToList();

      if (neighborCells.Count == 0) return false;

      Color baseColor;
      try { baseColor = centerStack.GetLastCellColor(); }
      catch { return false; }

      var sameColorNeighbors = neighborCells
        .Where(n =>
        {
          var s = n.GetComponentInChildren<Stack>();
          return s != null && s.GetLastCellColor() == baseColor;
        })
        .ToList();

      if (sameColorNeighbors.Count == 0)
        return false;

      foreach (var neighbor in sameColorNeighbors)
      {
        var neighborStack = neighbor.GetComponentInChildren<Stack>();
        if (neighborStack == null) continue;

        var tilesToMove = GetCellsToMove(neighborStack, baseColor);
        if (tilesToMove.Count == 0) continue;

        MoveCellsToOtherStack(tilesToMove, centerStack);
        mergedAny = true;
      }

      if (mergedAny)
      {
        RecalcStackPositions(centerStack);
        if (recursiveCheck)
          CheckAllStacksForMerges();
      }

      return mergedAny;
    }

    private List<Cell> GetCellsToMove(Stack stack, Color color)
    {
      var result = new List<Cell>();
      if (stack == null || stack.Tiles == null) return result;

      for (int i = stack.Tiles.Count - 1; i >= 0; i--)
      {
        var go = stack.Tiles[i];
        if (go == null) break;

        var cell = go.GetComponent<Cell>();
        if (cell == null || cell.Color != color)
          break;

        result.Add(cell);
      }
      return result;
    }

    private void MoveCellsToOtherStack(List<Cell> cellsToMove, Stack targetStack)
    {
      for (int i = cellsToMove.Count - 1; i >= 0; i--)
      {
        var cell = cellsToMove[i];
        if (cell == null) continue;

        var prevStack = cell.GetComponentInParent<Stack>();
        if (prevStack == null) continue;

        prevStack.Remove(cell.gameObject);

        if (prevStack.Tiles.Count == 0)
          RemoveStack(prevStack);

        cell.SetParent(targetStack.transform);
        targetStack.Add(cell.gameObject);
      }

      RecalcStackPositions(targetStack);
    }

    private void RemoveStack(Stack stack)
    {
      if (stack == null) return;
      _stacksOnGrid.Remove(stack);
      var cell = stack.GetComponentInParent<Cell>();
      if (cell != null)
      {
        cell.SetEmpty(true);
        cell.ShineOff();
      }
      Destroy(stack.gameObject);
    }

    private void RecalcStackPositions(Stack stack)
    {
      if (stack == null || stack.Tiles == null) return;
      for (int i = 0; i < stack.Tiles.Count; i++)
      {
        var go = stack.Tiles[i];
        if (go == null) continue;
        go.transform.position = stack.transform.position + Vector3.up * (i * 0.5f);
      }
    }

    private List<Cell> GetNeighbors(Cell cell)
    {
      if (cell == null) return new List<Cell>();
      LayerMask mask = 1 << cell.gameObject.layer;
      var hits = Physics.OverlapSphere(cell.transform.position, 5f, mask);
      var result = hits
        .Select(h => h.GetComponent<Cell>())
        .Where(c => c != null && c != cell)
        .ToList();
      return result;
    }
  }
}