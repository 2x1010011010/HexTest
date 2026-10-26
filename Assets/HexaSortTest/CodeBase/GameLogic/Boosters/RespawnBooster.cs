using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Boosters
{
  public class RespawnBooster : MonoBehaviour, IBooster
  {
    public BoosterType Type => BoosterType.Respawn;
  }
}
