using System;
using TMPro;
using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace HexaSortTest.CodeBase.GameLogic.UI.Meta
{
  public class MetaTileListItemButton : ButtonBase
  {
    [SerializeField, BoxGroup("ITEM SETUP")] private Image _icon;
    [SerializeField, BoxGroup("ITEM SETUP")] private TMP_Text _title;
    [SerializeField, BoxGroup("ITEM SETUP")] private TMP_Text _progressText;
    [SerializeField, BoxGroup("ITEM SETUP")] private GameObject _selectedRoot;
    [SerializeField, BoxGroup("LOCK SETTINGS")] private GameObject _lockRoot;
    [SerializeField, BoxGroup("LOCK SETTINGS")] private CanvasGroup _canvasGroup;
    [SerializeField, BoxGroup("LOCK SETTINGS"), Range(0f, 1f)] private float _lockedAlpha = 0.4f;

    private bool _locked;
    public event Action<MetaTileListItemButton> OnItemButtonClick;
    public MetaTileConfig Config { get; private set; }

    public void Setup(MetaTileConfig config)
    {
      Config = config;
      if (_title != null) _title.text = config?.DisplayName ?? "Location";
      if (_icon != null)
      {
        _icon.sprite = config?.Icon;
        _icon.enabled = _icon.sprite != null;
      }
      SetSelected(false);
      SetLocked(config?.TilePrefab == null);
    }

    public void SetLocked(bool locked)
    {
      _locked = locked;
      if (_button != null) _button.interactable = !locked;
      // Assign only the preview's CanvasGroup so the lock stays fully opaque.
      if (_canvasGroup != null) _canvasGroup.alpha = locked ? _lockedAlpha : 1f;
      if (_lockRoot != null) _lockRoot.SetActive(locked);
      if (_progressText != null) _progressText.gameObject.SetActive(!locked);
      if (locked) SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
      if (_selectedRoot != null) _selectedRoot.SetActive(selected && !_locked);
    }

    public void SetProgress(int opened, int total)
    {
      if (_progressText != null)
        _progressText.text = $"{Mathf.Clamp(opened, 0, Mathf.Max(0, total))}/{Mathf.Max(0, total)}";
    }

    protected override void ButtonClick()
    {
      if (_locked) return;
      if (AudioFacade.Instance != null) AudioFacade.Instance.PlayClick();
      OnItemButtonClick?.Invoke(this);
    }
  }
}
