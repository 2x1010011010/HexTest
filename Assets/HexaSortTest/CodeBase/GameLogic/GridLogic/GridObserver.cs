using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.Data;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;
using HexaSortTest.CodeBase.GameLogic.UI.HUD;
using Sirenix.OdinInspector;
using UnityEngine;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using HexaSortTest.CodeBase.GameLogic.UI;
using HexaSortTest.CodeBase.GameLogic.UI.MainMenu;
using HexaSortTest.CodeBase.Infrastructure.StateMachine;
using HexaSortTest.CodeBase.Infrastructure.StateMachine.States;
using Cysharp.Threading.Tasks;
using HexaSortTest.CodeBase.Infrastructure.StateMachine.States.CustomPayloadStructures;

namespace HexaSortTest.CodeBase.GameLogic.GridLogic
{
  public class GridObserver : MonoBehaviour
  {
    [SerializeField, BoxGroup("SETUP")] private HexGrid _grid;

    private readonly Dictionary<Cell, List<Cell>> _neighbors = new();
    private readonly HashSet<Stack> _stacksOnGrid = new();
    private Cell _lastAddedCell;
    private UIWindow _mainMenu;

    private GameStateMachine _gameStateMachine;
    private GameResultPopup _resultPopup;
    private bool _resultAlreadyTriggered;

    public void SetMainMenu(MainMenuObserver mainMenu) => _mainMenu = mainMenu;
    public void Init(HexGrid grid) => _grid = grid;

    public void SetGameResultHandler(GameStateMachine gameStateMachine, GameResultPopup resultPopup)
    {
      _gameStateMachine = gameStateMachine;
      _resultPopup = resultPopup;
      Debug.Log($"[GridObserver] SetGameResultHandler called on {gameObject.name} (instance {GetInstanceID()}). " +
                $"gameStateMachine={(_gameStateMachine != null)}, resultPopup={(_resultPopup != null)}");
    }

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

      CheckAllStacksForMergesAsync().Forget();
    }

    private async UniTaskVoid Update()
    {
      if (Input.GetMouseButtonUp(0))
      {
        if (RescanForNewStacks(out var newCell))
        {
          _lastAddedCell = newCell;
          await ProcessMergesFromCellAsync(_lastAddedCell);
          await CheckForLoseConditionAsync();
        }
      }
    }

    private void ScanAndRegisterStacks()
    {
      _stacksOnGrid.Clear();

      foreach (var cell in _grid.Cells)
      {
        var stack = cell.GetComponentInChildren<Stack>();
        if (stack != null && !stack.IsDragged)
          _stacksOnGrid.Add(stack);
      }

      CheckAllStacksForMergesAsync().Forget();
    }

    private bool RescanForNewStacks(out Cell newCell)
    {
      foreach (var cell in _grid.Cells)
      {
        var stack = cell.GetComponentInChildren<Stack>();
        if (stack == null || stack.IsDestroyed() || stack.IsDragged)
          continue;

        if (_stacksOnGrid.Contains(stack))
          continue;

        _stacksOnGrid.Add(stack);
        newCell = cell;
        return true;
      }

      newCell = null;
      return false;
    }

    private async UniTask CheckAllStacksForMergesAsync()
    {
      bool merged;

      do
      {
        merged = false;

        var stacks = _grid.Cells
          .Select(c => c.GetComponentInChildren<Stack>())
          .Where(s => s != null && !s.IsDragged)
          .ToList();

        foreach (var stack in stacks)
        {
          var cell = stack.Cell;
          if (cell == null || stack.IsDragged)
            continue;

          if (await ProcessMergesFromCellAsync(cell, recursiveCheck: false))
            merged = true;
        }
      } while (merged);

      await CheckAllStacksForColorThresholdAsync();
      await CheckForLoseConditionAsync();
    }

    private async UniTask<bool> ProcessMergesFromCellAsync(Cell centerCell, bool recursiveCheck = true)
    {
      if (centerCell == null)
        return false;

      var centerStack = centerCell.GetComponentInChildren<Stack>();
      if (centerStack == null || centerStack.IsDragged)
        return false;

      bool mergedAny = false;
      bool keepMerging;

      do
      {
        keepMerging = false;

        var neighborCells = _neighbors[centerCell]
          .Where(n =>
          {
            var stack = n.GetComponentInChildren<Stack>();
            return stack != null && !stack.IsDragged;
          })
          .ToList();

        if (neighborCells.Count == 0)
          break;

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
            return s.GetLastCellColor() == baseColor;
          })
          .ToList();

        if (sameColorNeighbors.Count == 0)
          break;

        foreach (var neighbor in sameColorNeighbors)
        {
          var neighborStack = neighbor.GetComponentInChildren<Stack>();
          if (neighborStack == null || neighborStack.IsDragged)
            continue;

          var tilesToMove = GetCellsToMove(neighborStack, baseColor);
          if (tilesToMove.Count == 0)
            continue;

          await MoveCellsToOtherStackAsync(tilesToMove, centerStack);

          mergedAny = true;
          keepMerging = true;
        }

        if (!centerStack.IsDragged)
          await centerStack.CheckForColorThreshold();

        centerStack = centerCell.GetComponentInChildren<Stack>();
      } while (keepMerging && centerStack != null && !centerStack.IsDragged && centerStack.Tiles.Count > 0);

      if (mergedAny && recursiveCheck)
        await CheckAllStacksForMergesAsync();

      return mergedAny;
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

    private async UniTask MoveCellsToOtherStackAsync(List<StackTile> cellsToMove, Stack targetStack)
    {
      if (cellsToMove == null || targetStack == null || targetStack.IsDragged)
        return;

      List<GameObject> movedTiles = new List<GameObject>();
      Vector3 moveDirection = Vector3.forward;
      Stack prevStack = null;

      for (int i = cellsToMove.Count - 1; i >= 0; i--)
      {
        var tile = cellsToMove[i];
        if (tile == null)
          continue;

        prevStack = tile.GetComponentInParent<Stack>();
        if (prevStack == null || prevStack.IsDragged)
          continue;

        prevStack.Remove(tile.gameObject);

        if (prevStack.Tiles.Count == 0)
          RemoveStack(prevStack);

        moveDirection = (targetStack.transform.position - prevStack.transform.position).normalized;

        movedTiles.Add(tile.gameObject);
      }

      await targetStack.AnimateMoveToStack(movedTiles, moveDirection);
    }

    private void RemoveStack(Stack stack)
    {
      if (stack == null)
        return;

      _stacksOnGrid.Remove(stack);

      var cell = stack.Cell;
      if (cell != null)
      {
        cell.SetEmpty(true);
        cell.ShineOff();
      }

      Destroy(stack.gameObject);
    }

    private async UniTask CheckAllStacksForColorThresholdAsync()
    {
      foreach (var stack in _stacksOnGrid.ToList())
      {
        if (stack != null && !stack.IsDragged)
          await stack.CheckForColorThreshold();
      }
    }

    private List<Cell> GetNeighbors(Cell cell)
    {
      if (cell == null)
        return new List<Cell>();

      LayerMask mask = 1 << cell.gameObject.layer;

      var hits = Physics.OverlapSphere(cell.transform.position, 5f, mask);

      return hits
        .Select(h => h.GetComponent<Cell>())
        .Where(c => c != null && c != cell)
        .ToList();
    }

    private async UniTask CheckForLoseConditionAsync()
    {
      if (_resultAlreadyTriggered)
        return;

      bool allFilled = _grid.Cells.All(c => !c.IsEmpty);

      if (!allFilled)
        return;

      foreach (var cell in _grid.Cells)
      {
        var stack = cell.GetComponentInChildren<Stack>();
        if (stack == null)
          continue;

        Color color;

        try
        {
          color = stack.GetLastCellColor();
        }
        catch
        {
          continue;
        }

        foreach (var neighbor in _neighbors[cell])
        {
          var neighborStack = neighbor.GetComponentInChildren<Stack>();
          if (neighborStack == null)
            continue;

          if (neighborStack.GetLastCellColor() == color)
            return;
        }
      }

      TriggerDefeat();
    }

    /// <summary>
    /// Called by HudObserver when the win-condition tile count is reached.
    /// </summary>
    public void TriggerVictory()
    {
      Debug.Log($"[GridObserver] TriggerVictory called on {gameObject.name}. alreadyTriggered={_resultAlreadyTriggered}");

      if (_resultAlreadyTriggered)
        return;

      _resultAlreadyTriggered = true;
      EnterGameResultState(isVictory: true);
    }

    private void TriggerDefeat()
    {
      Debug.Log($"[GridObserver] TriggerDefeat called on {gameObject.name}. alreadyTriggered={_resultAlreadyTriggered}");

      _resultAlreadyTriggered = true;
      EnterGameResultState(isVictory: false);
    }

    private void EnterGameResultState(bool isVictory)
    {
      Debug.Log($"[GridObserver] EnterGameResultState isVictory={isVictory}. " +
                $"gameStateMachine={(_gameStateMachine != null)}, resultPopup={(_resultPopup != null)}");

      if (_gameStateMachine == null || _resultPopup == null)
      {
        Debug.LogError("GridObserver: GameStateMachine/ResultPopup not set, falling back to main menu.");
        _mainMenu?.Open();
        return;
      }

      _gameStateMachine.Enter<GameResultState, GameResultPayload>(
        new GameResultPayload(isVictory, _resultPopup));
    }

    public void RemoveStackFromCellByBooster(Cell cell) =>
      _stacksOnGrid.Remove(cell.GetComponentInChildren<Stack>());
  }
}
