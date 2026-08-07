using System;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;

namespace HexaSortTest.CodeBase.GameLogic.UI.Meta
{
  public class MetaExitButton : ButtonBase
  {
    public event Action OnExitButtonClick;

    protected override void ButtonClick()
    {
      AudioFacade.Instance.PlayClick();
      OnExitButtonClick?.Invoke();
    }
  }
}