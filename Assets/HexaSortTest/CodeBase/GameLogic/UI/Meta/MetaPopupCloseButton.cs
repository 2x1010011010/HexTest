using System;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;
using UnityEngine.UI;

namespace HexaSortTest.CodeBase.GameLogic.UI.Meta
{
  public sealed class MetaPopupCloseButton : ButtonBase
  {
    public event Action OnCloseButtonClick;

    // For the fallback UI: configure while the object is inactive, before OnEnable.
    public void Configure(Button button) => _button = button;

    protected override void ButtonClick()
    {
      if (AudioFacade.Instance != null) AudioFacade.Instance.PlayClick();
      OnCloseButtonClick?.Invoke();
    }
  }
}
