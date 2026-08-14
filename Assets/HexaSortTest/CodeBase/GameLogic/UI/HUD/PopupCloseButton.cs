using System;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public class PopupCloseButton : ButtonBase
  {
    public event Action OnCloseClicked;

    protected override void ButtonClick()
    {
      AudioFacade.Instance.PlayClick();
      OnCloseClicked?.Invoke();
    }
  }
}