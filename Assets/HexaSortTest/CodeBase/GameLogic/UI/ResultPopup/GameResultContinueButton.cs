using System;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.ResultPopup
{
  public class GameResultContinueButton : ButtonBase
  {
    public event Action OnContinueButtonClick;

    protected override void ButtonClick()
    {
      Debug.Log("Game Result Continue Button pressed");
      AudioFacade.Instance.PlayClick();
      OnContinueButtonClick?.Invoke();
    }
  }
}