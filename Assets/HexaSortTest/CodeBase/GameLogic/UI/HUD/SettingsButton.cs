using System;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public class SettingsButton : ButtonBase
  {
    public event Action OnSettingsButtonClick;

    protected override void ButtonClick()
    {
      AudioFacade.Instance.PlayClick();
      OnSettingsButtonClick?.Invoke();
    }
  }
}