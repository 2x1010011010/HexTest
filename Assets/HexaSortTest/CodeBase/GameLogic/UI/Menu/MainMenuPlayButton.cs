using System;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.Menu
{
  public class MainMenuPlayButton : ButtonBase
  {
    public event Action OnPlayButtonClick;

    protected override void ButtonClick()
    {
      Debug.Log("Main Menu Play Button pressed");
      AudioFacade.Instance.PlayClick();
      OnPlayButtonClick?.Invoke();
    }
  }
}