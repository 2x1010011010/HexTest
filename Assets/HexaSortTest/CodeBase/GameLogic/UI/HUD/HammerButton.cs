using System;
using HexaSortTest.CodeBase.GameLogic.Boosters;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public class HammerButton : ButtonBase
  {
    public event Action<IBooster> OnHammerButtonClick;
    
    [SerializeField, BoxGroup("BOOSTER")] private HammerBooster _hammerBooster;
    
    protected override void ButtonClick()
    {
      OnHammerButtonClick?.Invoke(_hammerBooster);
    }
  }
}