using HexaSortTest.CodeBase.GameLogic.StackLogic;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Boosters
{
  public class RespawnBooster : MonoBehaviour, IBooster
  {
    public async void BoosterAction(Stack target)
    {
      await target.BreakStackByHammer(0);
    }
  }
}