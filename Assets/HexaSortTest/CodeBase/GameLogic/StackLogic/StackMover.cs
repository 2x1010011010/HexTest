using UnityEngine;
using DG.Tweening;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using Sirenix.OdinInspector;

namespace HexaSortTest.CodeBase.GameLogic.StackLogic
{
  public class StackMover : MonoBehaviour
  {
    [SerializeField, BoxGroup("SETUP")] private Stack _stack;
    [SerializeField, BoxGroup("SETUP")] private LayerMask _gridLayer;
    [SerializeField, BoxGroup("SETUP")] private LayerMask _groundLayer;
    [SerializeField, BoxGroup("SETUP")] private LayerMask _cellLayer;
    [SerializeField, BoxGroup("DROP SETTINGS")] private float _verticalShift = 2.5f;

    private bool _isDragging = false;
    private Vector3 _startPosition;
    private RaycastHit _hit;
    
    public void Move()
    {
      if (_stack.Parent.GetComponentInParent<HexGrid>()) return;
      
      StartDrag();
      
      GetHit(_cellLayer);
      if (_hit.collider == null) return;
      
      if (_isDragging)
      {
        if (GetHit(_groundLayer) || GetHit(_cellLayer))
        {
          _stack.SetParent(_stack.DefaultParent);
          _stack.transform.position = new Vector3(_hit.point.x, _stack.transform.position.y, _hit.point.z);
        }
        else if (GetHit(_gridLayer))
        {
          _stack.transform.position = new Vector3(_hit.point.x, _stack.transform.position.y, _hit.point.z);
          CheckGridCell();
        }
      }
    }

    public void Drop()
    {
      _isDragging = false;
      var position = -Vector3.up * _verticalShift;
      foreach (var tile in _stack.Tiles)
        tile.transform.DOMove(position, 0.7f);
    }

    public void Click()
    {
      if (_stack.Parent.GetComponentInParent<HexGrid>()) return;
    }

    private void StartDrag()
    {
      _isDragging = true;
      _startPosition = _stack.Parent.position;
      _stack.transform.position = Vector3.up * _verticalShift;
    }

    private void MoveToParent()
    {
      if (_stack.Parent.GetComponentInParent<HexGrid>()) return;
      _stack.transform.DOMove(_startPosition, 0.7f);
    }

    private void CheckGridCell()
    {
      RaycastHit hitToCell;
      if (Physics.Raycast(_stack.transform.position, Vector3.down, out hitToCell, 100, _gridLayer))
      {
        if(hitToCell.collider == null) return;
        var cell = hitToCell.collider.GetComponent<Cell>();
        if (!cell.IsEmpty) return;
        
        _stack.SetParent(cell.transform);
      }
    }

    private Ray GetRay() => 
      Camera.main.ScreenPointToRay(Input.mousePosition);
    
    private bool GetHit(LayerMask layerMask) => 
      Physics.Raycast(GetRay(), out _hit, 100, layerMask);
  }
}