using System.Collections.Generic;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.StackLogic
{
  public class Stack : MonoBehaviour
  {
    public List<GameObject> _stack = new();
    private Transform _parent;
    private Transform _defaultParent;
    private Vector3 _startPosition;
    
    public IReadOnlyList<GameObject> Tiles => _stack;
    public Transform Parent => _parent;
    public Transform DefaultParent => _defaultParent;
    
    public void SetParent(Transform parent)
    {
      _parent = parent;
      if (_defaultParent == null)
      {
        _defaultParent = parent;
        _startPosition = _defaultParent.position;
      }
    }

    public void Add(GameObject cell) => _stack.Add(cell);
    public void Remove(GameObject cell) => _stack.Remove(cell);
    public void SetActive(bool active) => _stack.ForEach(go => go.SetActive(active));
    public void Clear() => _stack.Clear();
  }
}