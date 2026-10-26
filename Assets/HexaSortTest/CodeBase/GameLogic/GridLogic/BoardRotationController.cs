using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.GridLogic
{
  [DisallowMultipleComponent]
  public sealed class BoardRotationController : MonoBehaviour
  {
    [SerializeField, Min(0.01f)] private float _snapDuration = 0.18f;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private Vector3 _previousDirection;
    private bool _hasDirection;
    private float _deadZone;
    public Vector3 Center { get; private set; }
    public float InteractionRadius { get; private set; }
    public float Angle { get; private set; }

    public void Initialize(HexGrid grid, float spacing)
    {
      _initialPosition = transform.position;
      _initialRotation = transform.rotation;
      var bounds = new Bounds(grid.Cells[0].transform.position, Vector3.zero);
      foreach (var cell in grid.Cells) bounds.Encapsulate(cell.transform.position);
      Center = bounds.center;
      foreach (var cell in grid.Cells)
        InteractionRadius = Mathf.Max(InteractionRadius, Vector3.Distance(Center, cell.transform.position));
      InteractionRadius += spacing * 0.6f;
      _deadZone = spacing * 0.2f;
    }

    public bool TryGetPoint(Camera camera, Vector2 screen, out Vector3 point)
    {
      point = default;
      if (camera == null) return false;
      var ray = camera.ScreenPointToRay(screen);
      if (!new Plane(Vector3.up, Center).Raycast(ray, out float distance)) return false;
      point = ray.GetPoint(distance);
      return true;
    }

    public bool Contains(Vector3 point) => Vector3.Distance(point, Center) <= InteractionRadius;

    public void Begin(Vector3 point)
    {
      _previousDirection = Vector3.ProjectOnPlane(point - Center, Vector3.up);
      _hasDirection = _previousDirection.sqrMagnitude >= _deadZone * _deadZone;
    }

    public void Follow(Vector3 point)
    {
      var direction = Vector3.ProjectOnPlane(point - Center, Vector3.up);
      if (direction.sqrMagnitude < _deadZone * _deadZone)
      {
        _hasDirection = false;
        return;
      }
      if (_hasDirection)
        SetAngle(Angle + Vector3.SignedAngle(_previousDirection, direction, Vector3.up));
      _previousDirection = direction;
      _hasDirection = true;
    }

    public async UniTask SnapAsync(CancellationToken token)
    {
      float target = BoardRotationMath.Snap(Angle);
      await BoardTween.PlayAsync(DOTween.To(() => Angle, SetAngle, target, _snapDuration).SetEase(Ease.OutCubic), token);
      SetAngle(BoardRotationMath.Normalize(target));
      Physics.SyncTransforms();
    }

    public void SnapImmediately()
    {
      SetAngle(BoardRotationMath.Normalize(BoardRotationMath.Snap(Angle)));
      Physics.SyncTransforms();
    }

    private void SetAngle(float angle)
    {
      Angle = angle;
      var turn = Quaternion.AngleAxis(angle, Vector3.up);
      transform.SetPositionAndRotation(Center + turn * (_initialPosition - Center), turn * _initialRotation);
    }
  }
}
