using System;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.ResultPopup
{
  public class GameResultMainMenuButton : ButtonBase
  {
    public event Action OnMainMenuButtonClick;

    protected override void ButtonClick()
    {
      Debug.Log("Game Result Main Menu Button pressed");
      AudioFacade.Instance.PlayClick();
      OnMainMenuButtonClick?.Invoke();
    }
  }
}