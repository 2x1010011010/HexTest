using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HexaSortTest.CodeBase.GameLogic.StackLogic
{
  public class StackMover : MonoBehaviour
  {
    public event Action<Stack> OnStackParentChange;
    [SerializeField, BoxGroup("SETUP")] private Stack _stack;
    [SerializeField, BoxGroup("SETUP")] private LayerMask _gridLayer;
    [SerializeField, BoxGroup("DROP AND DRAG SETTINGS")] private float _verticalShift = 2.5f;
    [SerializeField, BoxGroup("DROP AND DRAG SETTINGS")] private float _movementSpeed = 250f;
    private readonly List<Collider> _disabledColliders = new();
    private Transform _originParent;
    private Cell _originCell;
    private Vector3 _originPosition;
    private Quaternion _originRotation;
    private Vector3 _pointerOffset;
    private float _dragY;
    private Plane _plane;
    private GridObserver _board;
    private bool _dragging;
    public Cell HoverCell { get; private set; }
    public bool IsHandDrag { get; private set; }
    public Stack Stack => _stack;

    private void Awake()
    {
      if (_stack == null) _stack = GetComponent<Stack>();
      // Old prefab events must not race the single board pointer owner.
      var legacyTrigger = GetComponent<EventTrigger>();
      if (legacyTrigger != null) legacyTrigger.enabled = false;
    }

    public bool BeginDrag(GridObserver board, Camera camera, Vector2 screen, bool hand)
    {
      if (_dragging || _stack.Tiles.Count == 0) return false;
      _originParent = _stack.Parent;
      _originCell = _stack.Cell;
      _originPosition = transform.position;
      _originRotation = transform.rotation;
      _plane = new Plane(Vector3.up, new Vector3(0f, board.Rotation.Center.y, 0f));
      if (!TryPoint(camera, screen, out var point)) return false;
      _pointerOffset = Vector3.ProjectOnPlane(_originPosition - point, Vector3.up);
      _dragY = Mathf.Max(_originPosition.y + _verticalShift, board.GetDragHeight());
      _board = board;
      IsHandDrag = hand;
      _dragging = true;
      _stack.SetDragged(true);
      transform.SetParent(null, true);
      foreach (var collider in GetComponentsInChildren<Collider>())
        if (collider.enabled) { _disabledColliders.Add(collider); collider.enabled = false; }
      AudioFacade.Instance?.PlayClick();
      Follow(camera, screen);
      return true;
    }

    public void Follow(Camera camera, Vector2 screen)
    {
      if (!_dragging || !TryPoint(camera, screen, out var point)) return;
      point += _pointerOffset;
      transform.position = new Vector3(point.x, _dragY, point.z);
      var cell = _board.FindPlacementCell(point);
      if (cell == _originCell) cell = null; // Returning to the source is a cancellation, not a paid hand use.
      if (HoverCell != cell)
      {
        HoverCell?.ShineOff();
        HoverCell = cell;
        HoverCell?.ShineOn();
      }
    }

    public async UniTask MoveToCellAsync(Cell cell, CancellationToken token)
    {
      HoverCell?.ShineOff();
      transform.SetParent(cell.transform, true);
      var destination = cell.transform.position + Vector3.up * 0.5f;
      float duration = Mathf.Clamp(Vector3.Distance(transform.position, destination) / Mathf.Max(1f, _movementSpeed), 0.05f, 0.25f);
      await BoardTween.PlayAsync(transform.DOMove(destination, duration).SetEase(Ease.OutQuad), token);
    }

    public void Commit(Cell cell)
    {
      _stack.SetParentCell(cell.transform);
      _originCell?.SetEmpty(true);
      cell.SetEmpty(false);
      EndDrag();
      AudioFacade.Instance?.PlayOpen();
      OnStackParentChange?.Invoke(_stack);
    }

    public void Cancel()
    {
      if (!_dragging) return;
      HoverCell?.ShineOff();
      _stack.SetParentCell(_originParent);
      transform.SetPositionAndRotation(_originPosition, _originRotation);
      _originCell?.SetEmpty(false);
      EndDrag();
    }

    private void EndDrag()
    {
      HoverCell = null;
      _dragging = false;
      _stack.SetDragged(false);
      foreach (var collider in _disabledColliders) if (collider != null) collider.enabled = true;
      _disabledColliders.Clear();
    }

    private bool TryPoint(Camera camera, Vector2 screen, out Vector3 point)
    {
      point = default;
      if (camera == null) return false;
      var ray = camera.ScreenPointToRay(screen);
      if (!_plane.Raycast(ray, out float distance)) return false;
      point = ray.GetPoint(distance);
      return true;
    }

    // Kept for serialized callbacks on old prefabs; only BoardInputController drives gestures.
    public void Move() { }
    public void Drop() { }
    public void Click() { }
  }
}
