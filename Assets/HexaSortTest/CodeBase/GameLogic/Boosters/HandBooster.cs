using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Boosters
{
  public class HandBooster : MonoBehaviour, IBooster
  {
    public BoosterType Type => BoosterType.Hand;
  }
}
