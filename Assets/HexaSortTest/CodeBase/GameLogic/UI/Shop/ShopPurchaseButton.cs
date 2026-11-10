using System;
namespace HexaSortTest.CodeBase.GameLogic.UI.Shop
{
  public sealed class ShopPurchaseButton : ButtonBase
  {
    public event Action Clicked;
    protected override void ButtonClick() => Clicked?.Invoke();
  }
}
