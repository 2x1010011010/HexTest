using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public class SettingsPanel : UIPanel
  {
    [SerializeField, BoxGroup("SETTINGS PANEL SETUP")] private float _panelOpenDuration = 0.5f;
    [SerializeField, BoxGroup("SETTINGS PANEL SETUP")] private float _closeYPosition;
    [SerializeField, BoxGroup("SETTINGS PANEL SETUP")] private float _openYPosition;
    [SerializeField, BoxGroup("SETTINGS PANEL SETUP")] private SettingsButton _settingsButton;
    private CanvasGroup _group;
    private Tween _transition;

    private void Awake()
    {
      _group = GetComponent<CanvasGroup>();
      if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
      transform.localPosition = new Vector3(transform.localPosition.x, _closeYPosition, transform.localPosition.z);
      _group.alpha = 0;
      _group.interactable = _group.blocksRaycasts = false;
      IsOpen = false;
    }
    private void OnEnable() { if (_settingsButton != null) _settingsButton.OnSettingsButtonClick += Action; }
    private void OnDisable()
    {
      if (_settingsButton != null) _settingsButton.OnSettingsButtonClick -= Action;
      _transition?.Kill();
    }
    private void Action() { if (IsOpen) Close(); else Open(); }
    public override void Open()
    {
      if (IsOpen) return;
      IsOpen = true;
      _transition?.Kill();
      _group.alpha = 1;
      _group.interactable = _group.blocksRaycasts = true;
      _transition = transform.DOLocalMoveY(_openYPosition, _panelOpenDuration).SetEase(Ease.OutQuad);
    }
    public override void Close()
    {
      if (!IsOpen) return;
      _transition?.Kill();
      _group.interactable = false;
      _transition = transform.DOLocalMoveY(_closeYPosition, _panelOpenDuration).SetEase(Ease.InQuad).OnComplete(() =>
      {
        IsOpen = false;
        _group.alpha = 0;
        _group.blocksRaycasts = false;
        // Keep the controller enabled so its settings-button subscription remains available.
      });
    }
  }
}
