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
    
    private bool RaycastNotInStack()
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit;
      LayerMask mask = LayerMask.GetMask("Cell");
      return Physics.Raycast(ray, out hit) && mask != (1 << hit.collider.gameObject.layer);
    }
  }
}