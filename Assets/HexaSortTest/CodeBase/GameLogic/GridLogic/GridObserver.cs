using System.Collections.Generic;
using System.Linq;
using HexaSortTest.CodeBase.GameLogic.Cells;
using Sirenix.OdinInspector;
using UnityEngine;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using HexaSortTest.CodeBase.GameLogic.UI;
using HexaSortTest.CodeBase.GameLogic.UI.MainMenu;
using HexaSortTest.CodeBase.Infrastructure.StateMachine;
using HexaSortTest.CodeBase.Infrastructure.StateMachine.States;
using Cysharp.Threading.Tasks;
using HexaSortTest.CodeBase.GameLogic.Data;
using HexaSortTest.CodeBase.Infrastructure.StateMachine.States.CustomPayloadStructures;

namespace HexaSortTest.CodeBase.GameLogic.GridLogic
{
  public class GridObserver : MonoBehaviour
  {
    [SerializeField, BoxGroup("SETUP")] private HexGrid _grid;

    private readonly Dictionary<Cell, List<Cell>> _neighbors = new();
    private readonly HashSet<Stack> _stacksOnGrid = new();
    private UIWindow _mainMenu;

    private GameStateMachine _gameStateMachine;
    private bool _resultAlreadyTriggered;

    private StackMergeProcessor _mergeProcessor;

    // Guards against a new settle being kicked off (e.g. from a new stack
    // landing on the grid) while the current merge queue is still draining.
    // All queued grid movements must finish before anything else reacts to
    // the grid state — this is what enforces "movements first" ordering.
    private bool _isSettling;

    public void SetMainMenu(MainMenuObserver mainMenu) => _mainMenu = mainMenu;
    public void Init(HexGrid grid) => _grid = grid;

    public void SetGameResultHandler(GameStateMachine gameStateMachine)
    {
      _gameStateMachine = gameStateMachine;
      Debug.Log($"[GridObserver] SetGameResultHandler called on {gameObject.name} (instance {GetInstanceID()}). " +
                $"gameStateMachine={(_gameStateMachine != null)}");
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

      _mergeProcessor = new StackMergeProcessor(_grid, _neighbors, OnStackRemoved);

      ScanAndRegisterStacks();

      InitialSettleAsync().Forget();
    }

    private async UniTaskVoid InitialSettleAsync()
    {
      await SettleGridAsync();
      await CheckForLoseConditionAsync();
    }

    private async UniTaskVoid Update()
    {
      // Nothing new should react to the grid (new drops, lose checks, etc.)
      // while a settle is already draining its movement queue.
      if (_isSettling)
        return;

      if (Input.GetMouseButtonUp(0))
      {
        if (RescanForNewStacks(out _))
        {
          await SettleGridAsync();
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

    private async UniTask SettleGridAsync()
    {
      if (_isSettling)
        return;

      _isSettling = true;
      try
      {
        await _mergeProcessor.SettleAsync();
      }
      finally
      {
        _isSettling = false;
      }
    }

    private void OnStackRemoved(Stack stack) =>
      _stacksOnGrid.Remove(stack);

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
                $"gameStateMachine={(_gameStateMachine != null)}");

      if (_gameStateMachine == null)
      {
        Debug.LogError("GridObserver: GameStateMachine not set, falling back to main menu.");
        _mainMenu?.Open();
        return;
      }

      _gameStateMachine.Enter<GameResultState, GameResultPayload>(
        new GameResultPayload(isVictory));
    }

    public void RemoveStackFromCellByBooster(Cell cell) =>
      _stacksOnGrid.Remove(cell.GetComponentInChildren<Stack>());
  }
}
