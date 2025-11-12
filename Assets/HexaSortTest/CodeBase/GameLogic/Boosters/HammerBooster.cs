using UnityEngine;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using Sirenix.OdinInspector;

namespace HexaSortTest.CodeBase.GameLogic.Boosters
{
  public class HammerBooster : MonoBehaviour, IBooster
  {
    [SerializeField, BoxGroup("SETUP")] private float _punchScale = 1.5f;
    [SerializeField, BoxGroup("SETUP")] private float _punchDuration = 0.4f;

    public async void BoosterAction(Stack target)
    {
      if (target == null) return;
      await target.BreakStackByHammer(_punchScale, _punchDuration);
    }
  }
}