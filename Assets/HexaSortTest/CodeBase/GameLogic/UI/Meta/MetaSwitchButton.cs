using System;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;

namespace HexaSortTest.CodeBase.GameLogic.UI.Meta
{
  public class MetaSwitchButton : ButtonBase
  {
    public event Action OnSwitchButtonClick;

    protected override void ButtonClick()
    {
      AudioFacade.Instance.PlayClick();
      OnSwitchButtonClick?.Invoke();
    }
  }
}