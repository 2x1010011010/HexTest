using DG.Tweening;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.Meta
{
  // Optional future effect. Return a Tween/Sequence; null skips the flight.
  // The interactor owns cancellation. Implementations must clean temporary coins in OnKill.
  public abstract class MetaCoinFlightEffect : MonoBehaviour
  {
    public abstract Tween Play(Transform source, Transform target);
  }
}
