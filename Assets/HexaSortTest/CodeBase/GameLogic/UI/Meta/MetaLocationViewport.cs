using UnityEngine;
using HexaSortTest.CodeBase.GameLogic.Meta;
namespace HexaSortTest.CodeBase.GameLogic.UI.Meta
{
  public sealed class MetaLocationViewport : MonoBehaviour
  {
    private Camera _camera; private Bounds _bounds; private bool _hasBounds;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation, _viewRotation;
    private float _originalSize, _originalFarClip;
    private readonly Vector3[] _corners = new Vector3[4];
    public void SetTile(MetaTile tile)
    {
      _hasBounds = false;
      if (tile == null) return;
      // Generated cells are XY sprite planes in their own local space, even
      // when the authoring grid or the entire location is rotated.
      Transform plane = tile.transform;
      foreach (var cell in tile.Cells)
        if (cell != null) { plane = cell.transform; break; }
      _viewRotation = Quaternion.LookRotation(plane.forward, plane.up);
      Quaternion inverse = Quaternion.Inverse(_viewRotation);
      foreach (var renderer in tile.GetComponentsInChildren<Renderer>(true))
      {
        if (renderer is ParticleSystemRenderer || renderer.GetComponentInParent<Canvas>() != null) continue;
        // Capture the full location, including unopened/hidden pieces, once.
        // Do not reframe as covers disappear or play their reveal animation.
        Bounds local = renderer.localBounds;
        for (int i = 0; i < 8; i++)
        {
          Vector3 corner = local.center + Vector3.Scale(local.extents,
            new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
          Vector3 point = inverse * renderer.transform.TransformPoint(corner);
          if (!_hasBounds) { _bounds = new Bounds(point, Vector3.zero); _hasBounds = true; }
          else _bounds.Encapsulate(point);
        }
      }
    }
    private void LateUpdate() => FitNow();

    // Also used before revealing the scene, after the Canvas layout is rebuilt.
    public bool FitNow()
    {
      if (!_hasBounds) return false;
      if (_camera == null)
      {
        _camera = Camera.main;
        if (_camera == null) return false;
        _originalPosition = _camera.transform.position;
        _originalRotation = _camera.transform.rotation;
        _originalSize = _camera.orthographicSize;
        _originalFarClip = _camera.farClipPlane;
      }
      ((RectTransform)transform).GetWorldCorners(_corners);
      var canvas = GetComponentInParent<Canvas>();
      if (canvas == null) return false;
      var uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
      var a = RectTransformUtility.WorldToScreenPoint(uiCamera, _corners[0]);
      var b = RectTransformUtility.WorldToScreenPoint(uiCamera, _corners[2]);
      var pixels = _camera.pixelRect;
      if (pixels.width <= 0 || pixels.height <= 0) return false;
      float w = (b.x-a.x) / pixels.width, h = (b.y-a.y) / pixels.height;
      if (w <= 0 || h <= 0) return false;
      _camera.transform.rotation = _viewRotation;
      Vector3 worldCenter = _viewRotation * _bounds.center;
      var right = _camera.transform.right; var up = _camera.transform.up;
      float ex = _bounds.extents.x, ey = _bounds.extents.y, ez = _bounds.extents.z;
      var center = ((a+b)*0.5f - pixels.position) / pixels.size;
      if (!_camera.orthographic)
      {
        var forward = _camera.transform.forward;
        float tanY = Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float tanX = tanY * _camera.aspect;
        float ox = 2 * center.x - 1, oy = 2 * center.y - 1;
        float distance = Mathf.Max(ex/(tanX*w) + ez*(1+Mathf.Abs(ox)/w),
          ey/(tanY*h) + ez*(1+Mathf.Abs(oy)/h)) * 1.08f;
        distance = Mathf.Max(distance, _camera.nearClipPlane + ez + 0.1f);
        _camera.farClipPlane = Mathf.Max(_originalFarClip, distance + ez + 1f);
        _camera.transform.position = worldCenter - forward*distance
          - right*(ox*distance*tanX) - up*(oy*distance*tanY);
        return true;
      }
      float size = Mathf.Max(ey/h, ex/(w*_camera.aspect))*1.08f;
      _camera.orthographicSize = Mathf.Max(0.1f, size);
      float depth = Mathf.Max(10f, ez + _camera.nearClipPlane + 1f);
      _camera.farClipPlane = Mathf.Max(_originalFarClip, depth + ez + 1f);
      var p = worldCenter - _camera.transform.forward * depth;
      p -= right*((center.x-.5f)*2*size*_camera.aspect) + up*((center.y-.5f)*2*size);
      _camera.transform.position = p;
      return true;
    }
    private void OnDisable()
    {
      if (_camera == null) return;
      _camera.transform.SetPositionAndRotation(_originalPosition, _originalRotation);
      _camera.orthographicSize = _originalSize;
      _camera.farClipPlane = _originalFarClip;
      _camera = null;
    }
  }
}
