using System;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;

namespace HexaSortTest.CodeBase.GameLogic.UI.Shop
{
  public class ShopExitButton : ButtonBase
  {
    public event Action OnExitButtonClick;

    protected override void ButtonClick()
    {
      AudioFacade.Instance.PlayClick();
      OnExitButtonClick?.Invoke();
    }
  }
}