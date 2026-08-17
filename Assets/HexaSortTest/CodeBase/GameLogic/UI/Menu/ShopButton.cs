using System;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;

namespace HexaSortTest.CodeBase.GameLogic.UI.Menu
{
  public class ShopButton : ButtonBase
  {
    public event Action OnShopButtonClick;

    protected override void ButtonClick()
    {
      AudioFacade.Instance.PlayClick();
      OnShopButtonClick?.Invoke();
    }
  }
}