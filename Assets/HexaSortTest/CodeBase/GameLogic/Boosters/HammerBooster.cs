using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Boosters
{
  public class HammerBooster : MonoBehaviour, IBooster
  {
    public BoosterType Type => BoosterType.Hammer;
  }
}
