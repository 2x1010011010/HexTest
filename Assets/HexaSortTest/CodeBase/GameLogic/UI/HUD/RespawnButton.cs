using System;
using HexaSortTest.CodeBase.GameLogic.Boosters;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public class RespawnButton : ButtonBase
  {
    public event Action<IBooster> OnRespawnButtonClick;
    
    [SerializeField, BoxGroup("BOOSTER")] private RespawnBooster _hammerBooster;
    
    protected override void ButtonClick()
    {
      OnRespawnButtonClick?.Invoke(_hammerBooster);
    }
  }
}