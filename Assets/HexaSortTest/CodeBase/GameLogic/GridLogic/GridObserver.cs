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
    private Cell _currentCell;


    public void Init(HexGrid grid) => _grid = grid;

    private void Start()
    {
      foreach (var cell in _grid.Cells)
      {
        _neighbors.Add(cell, new List<Cell>());
        _neighbors[cell] = GetNeighbors(cell);
        var stack = cell.GetComponentInChildren<Stack>();
        if (stack == null) continue;
        AddStack(stack);
      }

      foreach (var stack in _stacksOnGrid)
        CheckStacks(stack.GetComponentInParent<Cell>());
    }

    private void Update()
    {
      if (Input.GetMouseButtonUp(0))
      {
        if (!IsStackPlaced()) return;
        CheckStacks(_currentCell);
      }
    }

    private void AddStack(Stack stack) =>
      _stacksOnGrid.Add(stack);

    private void RemoveStack(Stack stack)
    {
      _stacksOnGrid.Remove(stack);
      var cell = stack.GetComponentInParent<Cell>();
      cell?.SetEmpty(true);
      cell?.ShineOff();
      Destroy(stack.gameObject);
    }

    private bool IsStackPlaced()
    {
      foreach (var cell in _grid.Cells)
      {
        var stack = cell.GetComponentInChildren<Stack>();
        if (stack == null) continue;
        if (_stacksOnGrid.Contains(stack)) continue;
        _stacksOnGrid.Add(stack);
        _currentCell = cell;
        return true;
      }

      return false;
    }

    private void CheckStacks(Cell gridCell)
    {
      Stack stack = gridCell.GetComponentInChildren<Stack>();
      Color color = stack.GetLastCellColor();
      List<Cell> neigbors = _neighbors[gridCell];
      List<Stack> neighborStacks = new();

      foreach (var neighbor in neigbors)
      {
        var neighborStack = neighbor.GetComponentInChildren<Stack>();
        if (stack == null) continue;
        neighborStacks.Add(neighborStack);
      }

      if (neighborStacks.Count == 0) return;

      foreach (var neighborStack in neighborStacks)
      {
        if (neighborStack.GetLastCellColor() == color)
        {
          List<Cell> cellsToMove = GetCellsToMove(neighborStack, color);
          MoveCellsToOtherStack(cellsToMove, stack);
        }
      }
    }

    private List<Cell> GetCellsToMove(Stack stack, Color color)
    {
      List<Cell> cellsToMove = new();

      for (int i = stack.Tiles.Count - 1; i >= 0; i--)
        if (stack.Tiles[i].GetComponent<Cell>().Color != color)
          break;
        else
          cellsToMove.Add(stack.Tiles[i].GetComponent<Cell>());

      return cellsToMove;
    }

    private void MoveCellsToOtherStack(List<Cell> cellsToMove, Stack stack)
    {
      foreach (var cell in cellsToMove)
      {
        var previousStack = cell.GetComponentInParent<Stack>();
        previousStack.Remove(cell.gameObject);
        
        if (previousStack.Tiles.Count == 0)
          RemoveStack(previousStack);
        
        cell.SetParent(stack.transform);
        stack.Add(cell.gameObject);
      }

      for (int i = 0; i < stack.Tiles.Count; i++)
      {
        stack.Tiles[i].transform.position = stack.transform.position + Vector3.up * (i * 0.5f);
      }
    }

    private List<Cell> GetNeighbors(Cell cell)
    {
      LayerMask layerMask = 1 << cell.gameObject.layer;
      var neighbors = Physics.OverlapSphere(cell.transform.position, 5, layerMask)
        .Select(hit => hit.GetComponent<Cell>()).ToList();
      neighbors.Remove(cell);
      return neighbors;
    }
  }
}