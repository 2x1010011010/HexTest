using System;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public class HandButton : ButtonBase
  {
    public event Action OnHandButtonClick;
    
    protected override void ButtonClick()
    {
      OnHandButtonClick?.Invoke();
    }
  }
}