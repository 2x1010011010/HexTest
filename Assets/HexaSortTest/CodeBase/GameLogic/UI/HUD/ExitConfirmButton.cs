using System;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public sealed class ExitConfirmButton : ButtonBase
  {
    public event Action Clicked;

    protected override void ButtonClick()
    {
      Clicked?.Invoke();
    }
  }
}
