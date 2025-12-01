using UnityEngine;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using Sirenix.OdinInspector;

namespace HexaSortTest.CodeBase.GameLogic.Boosters
{
  public class HammerBooster : MonoBehaviour, IBooster
  {
    public async void BoosterAction(Stack target)
    {
      if (target == null) return;
      await target.BreakStackByHammer();
    }
  }
}