using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.GridLogic
{
  public sealed class StackMergeProcessor
  {
    private readonly HexGrid _grid;
    private readonly Dictionary<Cell, List<Cell>> _neighbors;
    private readonly Func<Cell, int, CancellationToken, UniTask> _collected;

    public StackMergeProcessor(HexGrid grid, Dictionary<Cell, List<Cell>> neighbors,
      Func<Cell, int, CancellationToken, UniTask> collected)
    {
      _grid = grid;
      _neighbors = neighbors;
      _collected = collected;
    }

    public async UniTask SettleAsync(CancellationToken token)
    {
      bool collected;
      do
      {
        // Rebuild the decision after every completed transfer; queued targets can become stale.
        while (await MergeOneAsync(token)) { }
        collected = false;
        foreach (var cell in _grid.Cells)
        {
          token.ThrowIfCancellationRequested();
          var stack = GetStack(cell);
          if (stack == null) continue;
          int removed = await stack.CollectTopAsync(token);
          if (removed == 0) continue;
          collected = true;
          await _collected(cell, removed, token);
        }
      } while (collected);
    }

    private async UniTask<bool> MergeOneAsync(CancellationToken token)
    {
      token.ThrowIfCancellationRequested();
      for (int i = 0; i < _grid.Cells.Count; i++)
      {
        var cell = _grid.Cells[i];
        var stack = GetStack(cell);
        if (stack == null) continue;
        foreach (var neighbor in _neighbors[cell])
        {
          int j = _grid.Cells.IndexOf(neighbor);
          if (j <= i) continue;
          var other = GetStack(neighbor);
          if (other == null || stack.GetLastCellColor() != other.GetLastCellColor()) continue;
          bool hubA = IsHub(cell, stack.GetLastCellColor());
          bool hubB = IsHub(neighbor, other.GetLastCellColor());
          bool targetA = hubA != hubB ? hubA : stack.Tiles.Count >= other.Tiles.Count;
          var source = targetA ? other : stack;
          var target = targetA ? stack : other;
          var color = source.GetLastCellColor();
          var moved = new List<GameObject>();
          for (int t = source.Tiles.Count - 1; t >= 0; t--)
          {
            var tile = source.Tiles[t];
            if (tile.GetComponent<StackTile>().Color != color) break;
            moved.Add(tile);
          }
          moved.Reverse();
          var direction = (target.transform.position - source.transform.position).normalized;
          foreach (var tile in moved) source.Remove(tile);
          await target.AnimateMoveToStack(moved, direction, token);
          source.DestroyIfEmpty();
          return true;
        }
      }
      return false;
    }

    private bool IsHub(Cell cell, Color color) => _neighbors[cell].Count(c =>
    {
      var stack = GetStack(c);
      return stack != null && stack.GetLastCellColor() == color;
    }) >= 2;

    public static Stack GetStack(Cell cell)
    {
      if (cell == null || (cell.TryGetComponent<CellCoverData>(out var cover) && cover.IsBlocked)) return null;
      var stack = cell.GetComponentInChildren<Stack>();
      return stack != null && !stack.IsDragged && stack.Tiles.Count > 0 ? stack : null;
    }
  }
}
