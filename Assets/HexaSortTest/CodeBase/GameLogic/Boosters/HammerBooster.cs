using UnityEngine;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using Sirenix.OdinInspector;

namespace HexaSortTest.CodeBase.GameLogic.Boosters
{
  public class HammerBooster : MonoBehaviour, IBooster
  {
    public async void BoosterAction(Stack target)
    {
      Debug.Log("Hammer Booster Action");
      if (target == null) { Debug.Log("Stack is null"); return;}
      await target.BreakStackByHammer();
    }
  }
}