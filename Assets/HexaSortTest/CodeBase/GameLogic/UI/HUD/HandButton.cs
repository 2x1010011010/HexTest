using System;
using HexaSortTest.CodeBase.GameLogic.Boosters;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public class HandButton : ButtonBase
  {
    public event Action<IBooster> OnHandButtonClick;
    [SerializeField, BoxGroup("BOOSTER")] private HandBooster _handBooster;
    
    protected override void ButtonClick()
    {
      OnHandButtonClick?.Invoke(_handBooster);
    }
  }
}