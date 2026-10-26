using System.Collections.Generic;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HexaSortTest.CodeBase.GameLogic.GridLogic
{
  // Owns one captured pointer. UI raycasts deliberately exclude PhysicsRaycaster hits.
  [DisallowMultipleComponent]
  public sealed class BoardInputController : MonoBehaviour
  {
    private readonly List<RaycastResult> _uiHits = new();
    private GridObserver _board;
    private BoardRotationController _rotation;
    private Camera _camera;
    private int _fingerId;
    private bool _captured;
    private bool _touch;
    private bool _ignoreTouchesUntilReleased;

    public void Initialize(GridObserver board, BoardRotationController rotation)
    {
      _board = board;
      _rotation = rotation;
      _camera = Camera.main;
    }

    private void Update()
    {
      if (_board == null || _camera == null) return;
      if (_board.InputBlocked || Input.GetKeyDown(KeyCode.Escape))
      {
        Cancel();
        return;
      }
      if (Input.touchCount > 1)
      {
        _ignoreTouchesUntilReleased = true;
        Cancel();
        return;
      }
      if (_ignoreTouchesUntilReleased)
      {
        if (Input.touchCount == 0) _ignoreTouchesUntilReleased = false;
        return;
      }
      if (_captured)
      {
        if (_touch)
        {
          bool found = false;
          for (int i = 0; i < Input.touchCount; i++)
          {
            var touch = Input.GetTouch(i);
            if (touch.fingerId != _fingerId) continue;
            found = true;
            if (touch.phase == TouchPhase.Canceled) Cancel();
            else if (touch.phase == TouchPhase.Ended) Release(touch.position);
            else Follow(touch.position);
            break;
          }
          if (!found) Cancel();
        }
        else if (Input.GetMouseButtonUp(0)) Release(Input.mousePosition);
        else if (Input.GetMouseButton(0)) Follow(Input.mousePosition);
        else Cancel();
        return;
      }
      if (Input.touchCount > 0)
      {
        var touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
          _touch = true;
          _fingerId = touch.fingerId;
          Press(touch.position);
        }
      }
      else if (!Application.isMobilePlatform && Input.GetMouseButtonDown(0))
      {
        _touch = false;
        Press(Input.mousePosition);
      }
    }

    private void Press(Vector2 screen)
    {
      if (IsOverUI(screen) || _board.InputBlocked) return;
      var hits = Physics.RaycastAll(_camera.ScreenPointToRay(screen), 1000f, ~0, QueryTriggerInteraction.Collide);
      System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
      Stack stack = null;
      foreach (var hit in hits)
      {
        stack = hit.collider.GetComponentInParent<Stack>();
        if (stack != null) break;
      }
      if (_board.State == BoardState.SelectingBooster)
      {
        _captured = _board.Boosters != null && _board.Boosters.TryChooseTarget(stack, screen, _camera);
        return;
      }
      if (_board.State != BoardState.Idle) return;
      if (stack != null && _board.IsTrayStack(stack))
      {
        _captured = _board.TryBeginDrag(stack, screen, _camera, false);
        return;
      }
      if (_rotation.TryGetPoint(_camera, screen, out var point) && _rotation.Contains(point))
      {
        _captured = _board.TryBeginRotation(point);
      }
    }

    private void Follow(Vector2 screen)
    {
      if (_board.State == BoardState.DraggingStack) _board.FollowDrag(screen, _camera);
      else if (_board.State == BoardState.Rotating && _rotation.TryGetPoint(_camera, screen, out var point))
        _rotation.Follow(point);
    }

    private void Release(Vector2 screen)
    {
      Follow(screen);
      _captured = false;
      if (_board.State == BoardState.DraggingStack) _board.ReleaseDrag(IsOverUI(screen));
      else if (_board.State == BoardState.Rotating) _board.ReleaseRotation();
    }

    private bool IsOverUI(Vector2 screen)
    {
      if (EventSystem.current == null) return false;
      _uiHits.Clear();
      EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = screen }, _uiHits);
      foreach (var hit in _uiHits)
        if (hit.module is GraphicRaycaster && !hit.gameObject.transform.IsChildOf(_board.transform)) return true;
      return false;
    }

    public void Cancel()
    {
      _captured = false;
      if (_board != null) _board.CancelInteraction();
    }

    private void OnApplicationFocus(bool focused) { if (!focused) Cancel(); }
    private void OnApplicationPause(bool paused) { if (paused) Cancel(); }
    private void OnDisable() => Cancel();
  }
}
