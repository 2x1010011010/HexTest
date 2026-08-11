using System;
using HexaSortTest.CodeBase.GameConfigs;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace HexaSortTest.CodeBase.GameLogic.UI.Meta
{
  public class MetaTileListItemButton : ButtonBase
  {
    [SerializeField, BoxGroup("ITEM SETUP")] private Image _icon;
    [SerializeField, BoxGroup("ITEM SETUP")] private CanvasGroup _canvasGroup;
    [SerializeField, BoxGroup("LOCK SETTINGS")] private float _lockedAlpha = 0.4f;

    public event Action<MetaTileListItemButton> OnItemButtonClick;

    public MetaTileConfig Config { get; private set; }

    public void Setup(MetaTileConfig config)
    {
      Config = config;

      if (_icon != null && config?.Icon != null)
        _icon.sprite = config.Icon;

      SetLocked(false);
    }

    public void SetLocked(bool locked)
    {
      Button.interactable = !locked;

      if (_canvasGroup != null)
        _canvasGroup.alpha = locked ? _lockedAlpha : 1f;
    }

    protected override void ButtonClick() =>
      OnItemButtonClick?.Invoke(this);
  }
}