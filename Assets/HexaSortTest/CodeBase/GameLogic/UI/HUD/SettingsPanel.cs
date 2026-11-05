using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
  public class SettingsPanel : UIPanel
  {
    public enum SlideDirection { Right, Left, Up, Down }

    [SerializeField, BoxGroup("SETTINGS PANEL SETUP"), Min(0)] private float _panelOpenDuration = 0.5f;
    [SerializeField, BoxGroup("SETTINGS PANEL SETUP")] private SettingsButton _settingsButton;
    [SerializeField, BoxGroup("SLIDE")] private Vector2 _openPosition = new Vector2(0, 1);
    [SerializeField, BoxGroup("SLIDE"), Tooltip("The edge the buttons hide behind.")]
    private SlideDirection _hideDirection = SlideDirection.Right;
    [SerializeField, BoxGroup("SLIDE"), Min(0), Tooltip("Minimum travel distance in local UI units.")]
    private float _hideDistance = 300;
    [SerializeField, BoxGroup("SLIDE"), Tooltip("Increase travel if needed to move completely beyond the root Canvas edge.")]
    private bool _hideBeyondCanvas = true;
    [SerializeField, BoxGroup("SLIDE"), Min(0)] private float _edgePadding = 20;

    private RectTransform _rect;
    private CanvasGroup _group;
    private Tween _transition;
    private bool _targetOpen;
    private readonly Vector3[] _corners = new Vector3[4];

    private void Awake()
    {
      _rect = (RectTransform)transform;
      _group = GetComponent<CanvasGroup>();
      if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
      ResetClosed();
    }

    private void OnEnable()
    {
      if (_settingsButton != null) _settingsButton.OnSettingsButtonClick += Toggle;
    }

    private void OnDisable()
    {
      if (_settingsButton != null) _settingsButton.OnSettingsButtonClick -= Toggle;
      _transition?.Kill();
      if (_rect != null) ResetClosed();
    }

    private void Toggle() { if (_targetOpen) Close(); else Open(); }

    public override void Open()
    {
      if (_targetOpen) return;
      _transition?.Kill();
      Canvas.ForceUpdateCanvases();
      if (!IsOpen) _rect.anchoredPosition = HiddenPosition();
      _targetOpen = IsOpen = true;
      _group.alpha = 1;
      _group.interactable = _group.blocksRaycasts = true;
      _transition = _rect.DOAnchorPos(_openPosition, _panelOpenDuration)
        .SetEase(Ease.OutQuad).SetUpdate(true);
    }

    public override void Close()
    {
      if (!_targetOpen) return;
      _targetOpen = false;
      _transition?.Kill();
      _group.interactable = false;
      _transition = _rect.DOAnchorPos(HiddenPosition(), _panelOpenDuration)
        .SetEase(Ease.InQuad).SetUpdate(true).OnComplete(() =>
        {
          IsOpen = false;
          _group.alpha = 0;
          _group.blocksRaycasts = false;
        });
    }

    private void ResetClosed()
    {
      _targetOpen = IsOpen = false;
      _group.alpha = 0;
      _group.interactable = _group.blocksRaycasts = false;
      _rect.anchoredPosition = HiddenPosition();
    }

    private Vector2 HiddenPosition()
    {
      Vector2 direction = _hideDirection switch
      {
        SlideDirection.Left => Vector2.left,
        SlideDirection.Up => Vector2.up,
        SlideDirection.Down => Vector2.down,
        _ => Vector2.right
      };
      float distance = Mathf.Max(0, _hideDistance);
      var canvas = GetComponentInParent<Canvas>();
      if (_hideBeyondCanvas && canvas != null && _rect.parent is RectTransform parent)
      {
        var canvasRect = (RectTransform)canvas.rootCanvas.transform;
        canvasRect.GetWorldCorners(_corners);
        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        foreach (var corner in _corners)
        {
          Vector2 point = parent.InverseTransformPoint(corner);
          min = Vector2.Min(min, point);
          max = Vector2.Max(max, point);
        }
        var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(parent, _rect);
        Vector2 shift = _openPosition - _rect.anchoredPosition;
        float toEdge = _hideDirection switch
        {
          SlideDirection.Left => bounds.max.x + shift.x - min.x,
          SlideDirection.Up => max.y - bounds.min.y - shift.y,
          SlideDirection.Down => bounds.max.y + shift.y - min.y,
          _ => max.x - bounds.min.x - shift.x
        };
        distance = Mathf.Max(distance, toEdge + Mathf.Max(0, _edgePadding));
      }
      return _openPosition + direction * distance;
    }
  }
}
