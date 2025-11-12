using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using HexaSortTest.CodeBase.GameLogic.Data;
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

    private async void Update()
    {
      if (Input.GetMouseButtonUp(0))
      {
        if (RescanForNewStacks(out var newCell))
        {
          _lastAddedCell = newCell;
          await ProcessMergesFromCellAsync(_lastAddedCell);
        }
      }
    }

    private void ScanAndRegisterStacks()
    {
      _stacksOnGrid.Clear();

      foreach (var cell in _grid.Cells)
      {
        var stack = cell.GetComponentInChildren<Stack>();
        if (!stack.IsDestroyed())
          _stacksOnGrid.Add(stack);
      }

      _ = CheckAllStacksForMergesAsync();
    }

    private bool RescanForNewStacks(out Cell newCell)
    {
      foreach (var cell in _grid.Cells)
      {
        var stack = cell.GetComponentInChildren<Stack>();
        if (stack.IsDestroyed()) continue;
        if (_stacksOnGrid.Contains(stack)) continue;

        _stacksOnGrid.Add(stack);
        newCell = cell;
        return true;
      }

      newCell = null;
      return false;
    }

    private async Task CheckAllStacksForMergesAsync()
    {
      bool merged;
      do
      {
        merged = false;
        var stacks = _grid.Cells
          .Select(c => c.GetComponentInChildren<Stack>())
          .Where(s => !s.IsDestroyed())
          .ToList();

        foreach (var stack in stacks)
        {
          var cell = stack.Cell;
          if (cell == null || cell.IsDestroyed()) continue;

          if (await ProcessMergesFromCellAsync(cell, recursiveCheck: false))
            merged = true;
        }
      } while (merged);

      await CheckAllStacksForColorThresholdAsync();
    }

    private async Task<bool> ProcessMergesFromCellAsync(Cell centerCell, bool recursiveCheck = true)
    {
      if (centerCell.IsDestroyed()) return false;
      var centerStack = centerCell.GetComponentInChildren<Stack>();
      if (centerStack.IsDestroyed()) return false;

      bool mergedAny = false;

      bool keepMerging;
      do
      {
        keepMerging = false;

        var neighborCells = _neighbors[centerCell]
          .Where(n => n != null && !n.IsDestroyed() && n.GetComponentInChildren<Stack>() != null)
          .ToList();

        if (neighborCells.Count == 0) break;

        Color baseColor;
        try
        {
          baseColor = centerStack.GetLastCellColor();
        }
        catch
        {
          break;
        }

        var sameColorNeighbors = neighborCells
          .Where(n =>
          {
            var s = n.GetComponentInChildren<Stack>();
            return !s.IsDestroyed() && s.GetLastCellColor() == baseColor;
          })
          .ToList();

        if (sameColorNeighbors.Count == 0) break;

        foreach (var neighbor in sameColorNeighbors)
        {
          var neighborStack = neighbor.GetComponentInChildren<Stack>();
          if (neighborStack.IsDestroyed()) continue;

          var tilesToMove = GetCellsToMove(neighborStack, baseColor);
          if (tilesToMove.Count == 0) continue;

          await MoveCellsToOtherStackSequentialAsync(tilesToMove, centerStack);

          mergedAny = true;
          keepMerging = true;
        }

        if (!centerStack.IsDestroyed())
          await centerStack.CheckForColorThreshold();

        if (centerCell.IsDestroyed()) break;
        centerStack = centerCell.GetComponentInChildren<Stack>();
        if (centerStack.IsDestroyed()) break;
      } while (keepMerging && !centerStack.IsDestroyed() && centerStack.Tiles.Count > 0);

      if (mergedAny && recursiveCheck)
        await CheckAllStacksForMergesAsync();

      return mergedAny;
    }

    private List<Cell> GetCellsToMove(Stack stack, Color color)
    {
      var result = new List<Cell>();
      if (stack.IsDestroyed() || stack.Tiles == null) return result;

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

    private async Task MoveCellsToOtherStackSequentialAsync(List<Cell> cellsToMove, Stack targetStack)
    {
      if (cellsToMove == null || targetStack.IsDestroyed()) return;

      foreach (var cell in cellsToMove)
      {
        if (cell.IsDestroyed()) continue;

        var prevStack = cell.GetComponentInParent<Stack>();
        if (prevStack.IsDestroyed()) continue;

        prevStack.Remove(cell.gameObject);

        if (prevStack.Tiles.Count == 0)
          RemoveStack(prevStack);

        cell.SetParent(targetStack.transform);
        targetStack.Add(cell.gameObject);

        Vector3 startPosition = cell.transform.position;
        Vector3 targetPosition = targetStack.transform.position +
                                 Vector3.up * (0.5f * targetStack.Tiles.IndexOf(cell.gameObject));
        Vector3[] path = new Vector3[]
        {
          startPosition,
          startPosition + Vector3.up * 2f,
          targetPosition + Vector3.up * 2f,
          targetPosition
        };

        Quaternion startRotation = cell.transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation((targetPosition - startPosition).normalized) *
                                    Quaternion.Euler(270f, 90f, 0f);

        var tcs = new TaskCompletionSource<bool>();

        cell.transform.DOPath(path, 0.5f, PathType.CatmullRom)
          .SetEase(Ease.InOutSine);

        cell.transform.DORotateQuaternion(targetRotation, 0.5f)
          .SetEase(Ease.InOutSine)
          .OnComplete(() =>
          {
            if (!cell.IsDestroyed())
              cell.transform.rotation = Quaternion.Euler(90f, 90f, 0f);
            tcs.TrySetResult(true);
          });

        await tcs.Task;
      }
    }

    private void RemoveStack(Stack stack)
    {
      if (stack.IsDestroyed()) return;

      _stacksOnGrid.Remove(stack);

      var cell = stack.Cell;
      if (cell != null && !cell.IsDestroyed())
      {
        cell.SetEmpty(true);
        cell.ShineOff();
      }

      Destroy(stack.gameObject);
    }

    private async Task CheckAllStacksForColorThresholdAsync()
    {
      foreach (var stack in _stacksOnGrid.ToList())
      {
        if (!stack.IsDestroyed())
          await stack.CheckForColorThreshold();
      }
    }

    private List<Cell> GetNeighbors(Cell cell)
    {
      if (cell == null || cell.IsDestroyed()) return new List<Cell>();

      LayerMask mask = 1 << cell.gameObject.layer;
      var hits = Physics.OverlapSphere(cell.transform.position, 5f, mask);

      return hits
        .Select(h => h.GetComponent<Cell>())
        .Where(c => c != null && c != cell)
        .ToList();
    }
  }
}