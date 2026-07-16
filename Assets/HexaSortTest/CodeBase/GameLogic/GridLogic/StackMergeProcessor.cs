using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.GridLogic
{
  public class StackMergeProcessor
  {
    private readonly HexGrid _grid;
    private readonly Dictionary<Cell, List<Cell>> _neighbors;
    private readonly Action<Stack> _onStackRemoved;

    public StackMergeProcessor(HexGrid grid, Dictionary<Cell, List<Cell>> neighbors,
      Action<Stack> onStackRemoved = null)
    {
      _grid = grid;
      _neighbors = neighbors;
      _onStackRemoved = onStackRemoved;
    }

    private readonly struct MergeAction
    {
      public readonly Cell Source;
      public readonly Cell Target;

      public MergeAction(Cell source, Cell target)
      {
        Source = source;
        Target = target;
      }
    }

    public async UniTask SettleAsync()
    {
      bool anyMerge;

      do
      {
        anyMerge = await RunMergePassAsync();
      } while (anyMerge);

      await RunThresholdPassAsync();
    }

    private async UniTask<bool> RunMergePassAsync()
    {
      List<MergeAction> queue = BuildMergeQueue();

      if (queue.Count == 0)
        return false;

      foreach (var action in queue)
        await ExecuteMergeAsync(action.Source, action.Target);

      return true;
    }

    private List<MergeAction> BuildMergeQueue()
    {
      var queue = new List<MergeAction>();
      var processedPairs = new HashSet<(Cell, Cell)>();
      var cellIndex = BuildCellIndex();

      foreach (var cell in _grid.Cells)
      {
        var stack = GetStack(cell);
        Color? color = TryGetColor(stack);
        if (color == null)
          continue;

        bool cellIsHub = IsHub(cell, color.Value);

        foreach (var neighbor in _neighbors[cell])
        {
          if (!cellIndex.TryGetValue(neighbor, out int neighborIndex) || neighborIndex <= cellIndex[cell])
            continue;

          if (!processedPairs.Add((cell, neighbor)))
            continue;

          var neighborStack = GetStack(neighbor);
          Color? neighborColor = TryGetColor(neighborStack);
          if (neighborColor == null || neighborColor.Value != color.Value)
            continue;

          bool neighborIsHub = IsHub(neighbor, neighborColor.Value);

          Cell target = ResolveTarget(cell, cellIsHub, stack, neighbor, neighborIsHub, neighborStack, cellIndex);
          Cell source = target == cell ? neighbor : cell;

          queue.Add(new MergeAction(source, target));
        }
      }

      return queue;
    }
    
    private bool IsHub(Cell cell, Color color)
    {
      int matchingNeighbors = 0;

      foreach (var neighbor in _neighbors[cell])
      {
        Color? neighborColor = TryGetColor(GetStack(neighbor));
        if (neighborColor != null && neighborColor.Value == color)
          matchingNeighbors++;

        if (matchingNeighbors >= 2)
          return true;
      }

      return false;
    }

    private Cell ResolveTarget(
      Cell cellA, bool cellAIsHub, Stack stackA,
      Cell cellB, bool cellBIsHub, Stack stackB,
      Dictionary<Cell, int> cellIndex)
    {
      if (cellAIsHub && !cellBIsHub)
        return cellA;

      if (cellBIsHub && !cellAIsHub)
        return cellB;
      
      int countA = stackA.Tiles.Count;
      int countB = stackB.Tiles.Count;

      if (countA != countB)
        return countA > countB ? cellA : cellB;

      return cellIndex[cellA] < cellIndex[cellB] ? cellA : cellB;
    }

    private async UniTask ExecuteMergeAsync(Cell sourceCell, Cell targetCell)
    {
      var source = GetStack(sourceCell);
      var target = GetStack(targetCell);

      if (!IsValidPair(source, target))
        return;

      Color color = source.GetLastCellColor();
      var tilesToMove = GetCellsToMove(source, color);
      if (tilesToMove.Count == 0)
        return;

      await MoveCellsToOtherStackAsync(tilesToMove, source, target);
    }

    private bool IsValidPair(Stack source, Stack target)
    {
      if (source == null || target == null || source == target)
        return false;

      if (source.IsDragged || target.IsDragged)
        return false;

      Color? sourceColor = TryGetColor(source);
      Color? targetColor = TryGetColor(target);

      return sourceColor != null && targetColor != null && sourceColor.Value == targetColor.Value;
    }

    private List<StackTile> GetCellsToMove(Stack stack, Color color)
    {
      var result = new List<StackTile>();
      if (stack == null || stack.Tiles == null || stack.IsDragged)
        return result;

      for (int i = stack.Tiles.Count - 1; i >= 0; i--)
      {
        var go = stack.Tiles[i];
        if (go == null)
          break;

        var tile = go.GetComponent<StackTile>();
        if (tile.Color != color)
          break;

        result.Add(tile);
      }

      return result;
    }

    private async UniTask MoveCellsToOtherStackAsync(List<StackTile> cellsToMove, Stack sourceStack, Stack targetStack)
    {
      if (cellsToMove == null || cellsToMove.Count == 0 || targetStack == null || targetStack.IsDragged)
        return;

      List<GameObject> movedTiles = new List<GameObject>();
      Vector3 moveDirection = Vector3.forward;

      for (int i = cellsToMove.Count - 1; i >= 0; i--)
      {
        var tile = cellsToMove[i];
        if (tile == null)
          continue;

        sourceStack.Remove(tile.gameObject);
        moveDirection = (targetStack.transform.position - sourceStack.transform.position).normalized;
        movedTiles.Add(tile.gameObject);
      }

      if (sourceStack.Tiles.Count == 0)
        RemoveStack(sourceStack);

      await targetStack.AnimateMoveToStack(movedTiles, moveDirection);
    }

    private void RemoveStack(Stack stack)
    {
      if (stack == null)
        return;

      var cell = stack.Cell;
      if (cell != null)
      {
        cell.SetEmpty(true);
        cell.ShineOff();
      }

      _onStackRemoved?.Invoke(stack);
      UnityEngine.Object.Destroy(stack.gameObject);
    }

    private async UniTask RunThresholdPassAsync()
    {
      var stacksSnapshot = _grid.Cells
        .Select(GetStack)
        .Where(s => s != null && !s.IsDragged)
        .ToList();

      foreach (var stack in stacksSnapshot)
      {
        if (stack == null)
          continue;

        await stack.CheckForColorThreshold();
      }
    }

    private Dictionary<Cell, int> BuildCellIndex()
    {
      var index = new Dictionary<Cell, int>();
      for (int i = 0; i < _grid.Cells.Count; i++)
        index[_grid.Cells[i]] = i;
      return index;
    }

    private Stack GetStack(Cell cell) =>
      cell != null ? cell.GetComponentInChildren<Stack>() : null;

    private Color? TryGetColor(Stack stack)
    {
      if (stack == null || stack.Tiles == null || stack.Tiles.Count == 0 || stack.IsDragged)
        return null;

      try
      {
        return stack.GetLastCellColor();
      }
      catch
      {
        return null;
      }
    }
  }
}