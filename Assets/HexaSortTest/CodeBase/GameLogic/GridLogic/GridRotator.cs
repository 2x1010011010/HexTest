using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.GridLogic
{
  public class GridRotator : MonoBehaviour
  {
    [SerializeField, BoxGroup("SETUP")] private float _rotationSpeed = 0.3f;
    [SerializeField, BoxGroup("SETUP")] private Vector3 _axis = Vector3.up;

    private bool _isRotating;
    private Vector3 _lastInputPosition;

    private void Update()
    {
      HandleMouseInput();
    }

    private void HandleMouseInput()
    {
      if (Input.GetMouseButtonDown(0))
      {
        if (RaycastNotOnCell())
        {
          _isRotating = true;
          _lastInputPosition = Input.mousePosition;
        }
      }
      else if (Input.GetMouseButtonUp(0))
      {
        _isRotating = false;
      }

      if (_isRotating)
      {
        Vector3 delta = Input.mousePosition - _lastInputPosition;
        float rotationAmount = delta.x * _rotationSpeed;
        transform.Rotate(_axis, rotationAmount * -1, Space.World);
        _lastInputPosition = Input.mousePosition;
      }
    }

    private bool RaycastNotOnCell()
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out RaycastHit hit))
      {
        return hit.collider.gameObject.layer != LayerMask.NameToLayer("Cell");
      }

      return true;
    }
  }
}
