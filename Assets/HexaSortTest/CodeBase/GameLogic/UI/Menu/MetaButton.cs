using System;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.Menu
{
  public class MetaButton : ButtonBase
  {
    public event Action OnMetaButtonClick;

    protected override void ButtonClick()
    {
      AudioFacade.Instance.PlayClick();
      OnMetaButtonClick?.Invoke();
    }
  }
}