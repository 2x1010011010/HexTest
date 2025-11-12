using System;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.MainMenu
{
  public class RestartLevelButton : ButtonBase
  {
    public event Action OnRestartLevelButtonClick;
    
    protected override void ButtonClick()
    {
      OnRestartLevelButtonClick?.Invoke();  
    }
  }
}