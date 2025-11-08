using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.GridLogic
{
  public class GridRotator : MonoBehaviour
  {
    [SerializeField, BoxGroup("SETUP")] private float _angle;
    [SerializeField, BoxGroup("SETUP")] private Vector3 _axis;

    private void Rotate()
    {
      transform.Rotate(_axis, _angle);
    }
  }
}