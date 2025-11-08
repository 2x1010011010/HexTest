using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine.UIElements;

namespace HexaSortTest.CodeBase.GameLogic.StackLogic
{
  public class StackMover : MonoBehaviour
  {
    [SerializeField, BoxGroup("SETUP")] private Stack _stack;
    [SerializeField, BoxGroup("DROP SETTINGS")] private float _verticalShift = 0.8f;
    
    private bool _isDragging = false;
    
    public void Move()
    {
      if (!_isDragging) StartDrag();
      
      var mousePosition = Input.mousePosition;
      var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
      transform.position = worldPosition;
    }

    public void Drop()
    {
      MoveToParent();
      _isDragging = false;
      var position = -Vector3.up * _verticalShift;
      foreach (var tile in _stack.Tiles)
        tile.transform.DOMove(position, 0.7f);
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