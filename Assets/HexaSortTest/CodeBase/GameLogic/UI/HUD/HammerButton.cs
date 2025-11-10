using System;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public class HammerButton : ButtonBase
  {
    public event Action OnHammerButtonClick;
    
    protected override void ButtonClick()
    {
      OnHammerButtonClick?.Invoke();
    }
  }
}