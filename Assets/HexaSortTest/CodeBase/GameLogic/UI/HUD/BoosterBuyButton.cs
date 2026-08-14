using System;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public class BoosterBuyButton : ButtonBase
  {
    public event Action OnBuyClicked;

    protected override void ButtonClick()
    {
      AudioFacade.Instance.PlayClick();
      OnBuyClicked?.Invoke();
    }
  }
}