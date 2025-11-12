using UnityEngine;
using HexaSortTest.CodeBase.GameLogic.StackLogic;

namespace HexaSortTest.CodeBase.GameLogic.Boosters
{
  public class HandBooster : MonoBehaviour, IBooster
  {
    public void BoosterAction(Stack target)
    {
      if (target == null) return;
      var mover = target.GetComponent<StackMover>();
      if (mover == null)
      {
        return;
      }
      
      mover.StartDragFromBooster();
    }
  }
}