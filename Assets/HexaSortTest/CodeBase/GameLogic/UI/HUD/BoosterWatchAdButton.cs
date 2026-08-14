using System;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public class BoosterWatchAdButton : ButtonBase
  {
    public event Action OnWatchAdClicked;

    protected override void ButtonClick()
    {
      AudioFacade.Instance.PlayClick();
      OnWatchAdClicked?.Invoke();
    }
  }
}