using System;
using UnityEngine;
using DG.Tweening;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using Sirenix.OdinInspector;

namespace HexaSortTest.CodeBase.GameLogic.StackLogic
{
  public class StackMover : MonoBehaviour
  {
    public event Action<Stack> OnStackParentChange; 
    
    [SerializeField, BoxGroup("SETUP")] private Stack _stack;
    [SerializeField, BoxGroup("SETUP")] private LayerMask _gridLayer;
    [SerializeField, BoxGroup("SETUP")] private LayerMask _groundLayer;
    [SerializeField, BoxGroup("SETUP")] private LayerMask _cellLayer;

    [SerializeField, BoxGroup("DROP AND DRAG SETTINGS")] private float _verticalShift = 2.5f;
    [SerializeField, BoxGroup("DROP AND DRAG SETTINGS")] private float _movementSpeed = 50f;

    private bool _isDragging = false;
    private Vector3 _startPosition;
    private RaycastHit _hit;
    private Cell _currentGridCell;
    private Camera _camera;
    
    private void Awake() => _camera = Camera.main;

    public void Move()
    {
      if (!_isDragging)
        StartDrag();

      if (!_isDragging) return;
      if (!GetHit()) return;

      var position = new Vector3(_hit.point.x, _stack.transform.position.y, _hit.point.z);
      _stack.transform.position = Vector3.MoveTowards(_stack.transform.position, position, Time.deltaTime * _movementSpeed);
      CheckGridCell();
    }

    public void Drop()
    {
      _isDragging = false;
      MoveToParent();
    }

    public void Click()
    {
      if (_stack.Parent.GetComponentInParent<HexGrid>()) return;
      //TO DO Click on stack animation
    }

    private void StartDrag()
    {
      if (_stack.Parent.GetComponentInParent<HexGrid>()) return;
      _isDragging = true;
      _startPosition = _stack.Parent.position;
      _stack.transform.position = _stack.Parent.position + Vector3.up * _verticalShift;
    }

    private void CheckGridCell()
    {
      _currentGridCell?.ShineOff();
      if (Physics.Raycast(_stack.transform.position, Vector3.down, out var hitToCell, 100, _gridLayer))
      {
        if (hitToCell.collider.GetComponent<Cell>() == null) return;
        _stack.SetParent(_stack.DefaultParent);

        var cell = hitToCell.collider.GetComponent<Cell>();
        if (!cell.IsEmpty) return;
        
        cell.ShineOn();
        _currentGridCell = cell;
        _stack.SetParent(cell.transform);
      }
      else
      {
        _stack.SetParent(_stack.DefaultParent);
      }
    }

    private void MoveToParent()
    {
      var distance = Vector3.Distance(_stack.transform.position, _stack.Parent.position);
      var duration = distance / _movementSpeed;
      _stack.transform.DOMove(_stack.Parent.position + Vector3.up * 0.5f, duration).SetEase(Ease.Linear);
      _stack.Parent.GetComponent<Cell>()?.SetEmpty(false);
      if (_stack.Parent.GetComponent<Cell>()) OnStackParentChange?.Invoke(_stack);
    }

    private Ray GetRay() =>
      _camera.ScreenPointToRay(Input.mousePosition);

    private bool GetHit() =>
      Physics.Raycast(GetRay(), out _hit, 100);
  }
}