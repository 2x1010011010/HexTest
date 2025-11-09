using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace HexaSortTest.CodeBase.GameLogic.StackLogic
{
  public class StackMover : MonoBehaviour
  {
    [SerializeField, BoxGroup("SETUP")] private Stack _stack;
    [SerializeField, BoxGroup("SETUP")] private LayerMask _gridLayer;
    [SerializeField, BoxGroup("SETUP")] private LayerMask _groundLayer;
    [SerializeField, BoxGroup("SETUP")] private LayerMask _cellLayer;
    [SerializeField, BoxGroup("DROP SETTINGS")] private float _verticalShift = 0.8f;

    private bool _isDragging = false;
    
    public void Move()
    {
      RaycastHit hit;
      Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 500, _cellLayer);

      if (hit.collider == null) return;
      if (_isDragging)
      {
        
      }
    }

    public void Drop()
    {
      MoveToParent();
      _isDragging = false;
      var position = -Vector3.up * _verticalShift;
      foreach (var tile in _stack.Tiles)
        tile.transform.DOMove(position, 0.7f);
    }

    public void Click()
    {
      
    }

    private void StartDrag()
    {
      _isDragging = true;
      _stack.transform.position = Vector3.up * _verticalShift;
    }

    private void MoveToParent() => 
      _stack.transform.position = _stack.Parent.transform.position;
  }
}