using System.Threading.Tasks;
using HexaSortTest.CodeBase.GameLogic.Spawners;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Boosters
{
  public class RespawnBooster : MonoBehaviour, IBooster
  {
    public BoosterType Type => BoosterType.Respawn;

    public async Task BoosterAction(Stack target)
    {
      await target.BreakStackByHammer(0);
    }
  }
}