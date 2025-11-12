using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public class SettingsButton : ButtonBase
  {
    public event Action OnSettingsButtonClick;
    

    protected override void ButtonClick()
    {
      OnSettingsButtonClick?.Invoke();
    }
  }
}