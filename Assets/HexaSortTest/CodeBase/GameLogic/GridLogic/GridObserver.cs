using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using HexaSortTest.CodeBase.GameLogic.Boosters;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.Spawners;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using HexaSortTest.CodeBase.GameLogic.UI.MainMenu;
using HexaSortTest.CodeBase.Infrastructure.Logging;
using HexaSortTest.CodeBase.Infrastructure.StateMachine;
using HexaSortTest.CodeBase.Infrastructure.StateMachine.States;
using HexaSortTest.CodeBase.Infrastructure.StateMachine.States.CustomPayloadStructures;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.GridLogic
{
  public class GridObserver : MonoBehaviour
  {
    [SerializeField, BoxGroup("SETUP")] private HexGrid _grid;
    private readonly Dictionary<Cell, List<Cell>> _neighbors = new();
    private readonly BoardSession _session = new();
    private readonly CancellationTokenSource _lifetime = new();
    private CancellationToken _token;
    private void Awake() => _token = _lifetime.Token;
    private MainMenuObserver _mainMenu;
    private GameStateMachine _gameStateMachine;
    private StackMergeProcessor _merges;
    private StacksSpawner _spawner;
    private StackMover _drag;
    private Func<bool> _inputBlocked;
    private int _gridMask;
    public BoardState State => _session.State;
    public int Score => _session.Score;
    public bool InputBlocked => (_mainMenu != null && _mainMenu.IsOpen) || (_inputBlocked?.Invoke() ?? false);
    public bool CanChooseBooster => !InputBlocked && (State == BoardState.Idle || State == BoardState.SelectingBooster);
    public BoardRotationController Rotation { get; private set; }
    public BoosterTools Boosters { get; private set; }
    public event Action<int> ScoreChanged { add => _session.ScoreChanged += value; remove => _session.ScoreChanged -= value; }

    public void SetMainMenu(MainMenuObserver mainMenu) => _mainMenu = mainMenu;
    public void Init(HexGrid grid) => _grid = grid;
    public void SetGameResultHandler(GameStateMachine stateMachine) => _gameStateMachine = stateMachine;
    public void Configure(int goal, Func<bool> inputBlocked)
    {
      _session.ConfigureGoal(goal);
      _inputBlocked = inputBlocked;
    }
    public void AttachBoosters(BoosterTools tools, StacksSpawner spawner) { Boosters = tools; _spawner = spawner; }

    private void Start()
    {
      try
      {
        if (_grid == null) _grid = GetComponent<HexGrid>();
        if (_grid == null || _grid.Cells == null || _grid.Cells.Count == 0)
          throw new InvalidOperationException("The level has no grid cells.");
        int layer = LayerMask.NameToLayer("Grid");
        if (layer < 0) throw new InvalidOperationException("The Grid layer is missing.");
        _gridMask = 1 << layer;
        float spacing = BuildNeighbors();
        Rotation = GetComponent<BoardRotationController>();
        if (Rotation == null) Rotation = gameObject.AddComponent<BoardRotationController>();
        Rotation.Initialize(_grid, spacing);
        var input = GetComponent<BoardInputController>();
        if (input == null) input = gameObject.AddComponent<BoardInputController>();
        input.Initialize(this, Rotation);
        _merges = new StackMergeProcessor(_grid, _neighbors, OnCollectedAsync);
        _session.TryTransition(BoardState.Initializing, BoardState.Settling);
        ResolveAsync().Forget();
      }
      catch (Exception error) { HandleFailure(error); }
    }

    private float BuildNeighbors()
    {
      // Topology is built once from cell centers, independent of colliders and subsequent rotation.
      float spacing = float.PositiveInfinity;
      foreach (var a in _grid.Cells)
        foreach (var b in _grid.Cells)
          if (a != b)
          {
            float distance = Vector3.Distance(a.transform.position, b.transform.position);
            if (distance > 0.001f) spacing = Mathf.Min(spacing, distance);
          }
      if (float.IsPositiveInfinity(spacing)) spacing = 5f;
      foreach (var cell in _grid.Cells)
        _neighbors[cell] = _grid.Cells.Where(other => other != cell &&
          Vector3.Distance(cell.transform.position, other.transform.position) <= spacing * 1.1f).ToList();
      return spacing;
    }

    public bool IsTrayStack(Stack stack) => _spawner != null && _spawner.IsTrayStack(stack);
    public bool IsPlacedStack(Stack stack) => stack != null && stack.Cell != null &&
      _grid.Cells.Contains(stack.Cell) && StackMergeProcessor.GetStack(stack.Cell) == stack;

    public bool TryBeginDrag(Stack stack, Vector2 screen, Camera camera, bool hand)
    {
      if (InputBlocked || (hand ? !IsPlacedStack(stack) : !IsTrayStack(stack))) return false;
      var expected = hand ? BoardState.SelectingBooster : BoardState.Idle;
      if (!_session.TryTransition(expected, BoardState.DraggingStack)) return false;
      _drag = stack.GetComponent<StackMover>();
      if (_drag != null && _drag.BeginDrag(this, camera, screen, hand)) return true;
      CancelInteraction();
      return false;
    }

    public void FollowDrag(Vector2 screen, Camera camera) { if (State == BoardState.DraggingStack) _drag?.Follow(camera, screen); }
    public void ReleaseDrag(bool overUI)
    {
      if (State != BoardState.DraggingStack) return;
      if (overUI || _drag == null || _drag.HoverCell == null) { CancelInteraction(); return; }
      var cell = _drag.HoverCell;
      if (_session.TryTransition(BoardState.DraggingStack, BoardState.DroppingStack)) DropAsync(cell).Forget();
    }

    private async UniTask DropAsync(Cell cell)
    {
      try
      {
        await _drag.MoveToCellAsync(cell, _token);
        if (_drag.IsHandDrag && (Boosters == null || !Boosters.TrySpendHand()))
        {
          _drag.Cancel();
          _drag = null;
          Boosters?.ClearSelection();
          _session.TryTransition(BoardState.DroppingStack, BoardState.Idle);
          return;
        }
        _drag.Commit(cell);
        _drag = null;
        Boosters?.ClearSelection();
        _session.TryTransition(BoardState.DroppingStack, BoardState.Settling);
        await ResolveAsync();
      }
      catch (Exception error) { HandleFailure(error); }
    }

    public Cell FindPlacementCell(Vector3 point)
    {
      if (!Physics.Raycast(new Vector3(point.x, Rotation.Center.y + 100f, point.z), Vector3.down,
            out var hit, 200f, _gridMask, QueryTriggerInteraction.Collide)) return null;
      var cell = hit.collider.GetComponentInParent<Cell>();
      if (cell == null || !_grid.Cells.Contains(cell) || !cell.IsEmpty) return null;
      if (cell.TryGetComponent<CellCoverData>(out var cover) && cover.IsBlocked) return null;
      return cell;
    }

    public float GetDragHeight()
    {
      float height = Rotation.Center.y + 2.5f;
      foreach (var cell in _grid.Cells)
      {
        var stack = StackMergeProcessor.GetStack(cell);
        if (stack == null) continue;
        foreach (var renderer in stack.GetComponentsInChildren<Renderer>())
          height = Mathf.Max(height, renderer.bounds.max.y + 1f);
      }
      return height;
    }

    public bool TryBeginRotation(Vector3 point)
    {
      if (InputBlocked || !_session.TryTransition(BoardState.Idle, BoardState.Rotating)) return false;
      Rotation.Begin(point);
      return true;
    }
    public void ReleaseRotation()
    {
      if (_session.TryTransition(BoardState.Rotating, BoardState.SnappingRotation)) SnapAsync().Forget();
    }
    private async UniTask SnapAsync()
    {
      try
      {
        await Rotation.SnapAsync(_token);
        _session.TryTransition(BoardState.SnappingRotation, BoardState.Idle);
      }
      catch (Exception error) { HandleFailure(error); }
    }

    public void SelectBooster() => _session.TryTransition(BoardState.Idle, BoardState.SelectingBooster);
    public void CancelInteraction()
    {
      var state = State;
      if (state != BoardState.DraggingStack && state != BoardState.Rotating && state != BoardState.SelectingBooster) return;
      _drag?.Cancel();
      _drag = null;
      if (state == BoardState.Rotating) Rotation.SnapImmediately();
      Boosters?.ClearSelection();
      _session.TryTransition(state, BoardState.Idle);
    }

    public async UniTask ExecuteBoosterAsync(Func<CancellationToken, UniTask> action)
    {
      if (!CanChooseBooster || !_session.TryTransition(State, BoardState.ApplyingBooster)) return;
      try
      {
        await action(_token);
        _session.TryTransition(BoardState.ApplyingBooster, BoardState.Settling);
        await ResolveAsync();
      }
      catch (Exception error) { HandleFailure(error); }
    }

    public async UniTask OnCollectedAsync(Cell cell, int count, CancellationToken token)
    {
      _session.AddScore(count);
      await UpdateCoversAsync(token);
      if (!_neighbors.TryGetValue(cell, out var neighbors)) return;
      var hits = new List<UniTask>();
      foreach (var neighbor in neighbors)
        if (neighbor.TryGetComponent<CellCoverData>(out var cover)) hits.Add(cover.HitAsync(token));
      await UniTask.WhenAll(hits);
    }

    private async UniTask UpdateCoversAsync(CancellationToken token)
    {
      var tasks = new List<UniTask>();
      foreach (var cell in _grid.Cells)
        if (cell.TryGetComponent<CellCoverData>(out var cover)) tasks.Add(cover.UpdateScoreAsync(Score, token));
      await UniTask.WhenAll(tasks);
    }

    private async UniTask ResolveAsync()
    {
      try
      {
        await UpdateCoversAsync(_token);
        await _merges.SettleAsync(_token);
        _token.ThrowIfCancellationRequested();
        bool full = _grid.Cells.All(cell =>
          (cell.TryGetComponent<CellCoverData>(out var cover) && cover.IsBlocked) || StackMergeProcessor.GetStack(cell) != null);
        var result = _session.CompleteSettlement(full);
        if (result == BoardOutcome.None) return;
        if (_gameStateMachine != null)
          _gameStateMachine.Enter<GameResultState, GameResultPayload>(new GameResultPayload(result == BoardOutcome.Victory));
        else _mainMenu?.Open();
      }
      catch (Exception error) { HandleFailure(error); }
    }

    private void HandleFailure(Exception error)
    {
      if (_token.IsCancellationRequested) return;
      _session.Fail();
#if UNITY_EDITOR
      GameLogger.Error($"[GridObserver] Board operation stopped: {error}");
#endif
      _mainMenu?.Open();
    }

    private void OnDestroy()
    {
      _lifetime.Cancel();
      _session.Dispose();
      _lifetime.Dispose();
    }
  }
}
