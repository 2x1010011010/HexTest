using System;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;

namespace HexaSortTest.CodeBase.GameLogic.UI.Menu
{
  public class HomeButton : ButtonBase
  {
    public event Action OnHomeButtonClick;

    protected override void ButtonClick()
    {
      AudioFacade.Instance.PlayClick();
      OnHomeButtonClick?.Invoke();
    }
  }
}
