using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using HexaSortTest.CodeBase.GameLogic.Cells;
using Sirenix.OdinInspector;
using UnityEngine;
using HexaSortTest.CodeBase.GameLogic.StackLogic;

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
        if (stack != null)
          _stacksOnGrid.Add(stack);
      }

      _ = CheckAllStacksForMergesAsync();
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

    private async Task CheckAllStacksForMergesAsync()
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
          if (await ProcessMergesFromCellAsync(cell, recursiveCheck: false))
            merged = true;
        }
      } while (merged);

      await CheckAllStacksForColorThresholdAsync();
    }

    private async Task<bool> ProcessMergesFromCellAsync(Cell centerCell, bool recursiveCheck = true)
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
      try
      {
        baseColor = centerStack.GetLastCellColor();
      }
      catch
      {
        return false;
      }

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

        await MoveCellsToOtherStackAsync(tilesToMove, centerStack);
        mergedAny = true;
      }

      if (mergedAny)
        await CheckAllStacksForColorThresholdAsync();

      if (mergedAny && recursiveCheck)
        await CheckAllStacksForMergesAsync();

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

    private async Task MoveCellsToOtherStackAsync(List<Cell> cellsToMove, Stack targetStack)
    {
      if (cellsToMove == null || targetStack == null) return;

      List<GameObject> movedTiles = new List<GameObject>();
      Vector3 moveDirection = Vector3.forward;
      Stack prevStack = null;

      for (int i = cellsToMove.Count - 1; i >= 0; i--)
      {
        var cell = cellsToMove[i];
        if (cell == null) continue;

        prevStack = cell.GetComponentInParent<Stack>();
        if (prevStack == null) continue;

        prevStack.Remove(cell.gameObject);

        if (prevStack.Tiles.Count == 0)
          RemoveStack(prevStack);

        Vector3 direction = (targetStack.transform.position - prevStack.transform.position).normalized;

        movedTiles.Add(cell.gameObject);
        moveDirection = direction;
      }

      await RecalcStackPositionsAsync(targetStack, movedTiles, moveDirection);
    }

    private void RemoveStack(Stack stack)
    {
      if (stack == null) return;
      _stacksOnGrid.Remove(stack);
      var cell = stack.Cell;
      if (cell != null)
      {
        cell.SetEmpty(true);
        cell.ShineOff();
      }

      Destroy(stack.gameObject);
    }

    private Task RecalcStackPositionsAsync(Stack stack, List<GameObject> movedTiles, Vector3 moveDirection)
    {
      var tcs = new TaskCompletionSource<bool>();
      if (stack == null || movedTiles == null)
      {
        tcs.SetResult(true);
        return tcs.Task;
      }

      float delay = 0f;
      float pauseBetween = 0.3f;
      float moveDuration = 0.5f;

      int completed = 0;
      int total = movedTiles.Count;

      for (int i = movedTiles.Count - 1; i >= 0; i--)
      {
        var go = movedTiles[i];
        if (go == null)
        {
          completed++;
          continue;
        }

        var cell = go.GetComponent<Cell>();
        cell.SetParent(stack.transform);
        stack.Add(cell.gameObject);

        Vector3 targetPosition = stack.transform.position + Vector3.up * (0.5f * stack.Tiles.IndexOf(go));
        Vector3 startPosition = go.transform.position;
        Vector3 aboveOldStack = startPosition + Vector3.up * 2f;
        Vector3 aboveNewStack = targetPosition + Vector3.up * 2f;
        Vector3[] path = new Vector3[] { startPosition, aboveOldStack, aboveNewStack, targetPosition };

        Quaternion prefabRotation = Quaternion.Euler(90f, 90f, 0f);
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection) * Quaternion.Euler(270f, 90f, 0f);

        go.transform.DOPath(path, moveDuration, PathType.CatmullRom)
          .SetDelay(delay)
          .SetEase(Ease.InOutSine);

        go.transform.DORotateQuaternion(targetRotation, moveDuration)
          .SetDelay(delay)
          .SetEase(Ease.InOutSine)
          .OnComplete(() =>
          {
            go.transform.rotation = prefabRotation;
            completed++;
            if (completed >= total)
              tcs.TrySetResult(true);
          });

        delay += pauseBetween;
      }

      return tcs.Task;
    }

    private async Task CheckAllStacksForColorThresholdAsync()
    {
      var stacksSnapshot = _stacksOnGrid.ToList();
      foreach (var stack in stacksSnapshot)
      {
        if (stack != null)
          await stack.CheckForColorThreshold();
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
