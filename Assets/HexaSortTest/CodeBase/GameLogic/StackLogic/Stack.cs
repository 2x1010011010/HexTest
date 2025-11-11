using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using HexaSortTest.CodeBase.GameLogic.Cells;

namespace HexaSortTest.CodeBase.GameLogic.StackLogic
{
  public class Stack : MonoBehaviour
  {
    public List<GameObject> _stack = new();
    private Transform _parent;
    private Transform _defaultParent;

    public IReadOnlyList<GameObject> Tiles => _stack;
    public Transform Parent => _parent;
    public Transform DefaultParent => _defaultParent;

    public void SetParent(Transform parent)
    {
      _parent = parent;
      transform.SetParent(parent);
      
      if (_defaultParent == null)
        _defaultParent = parent;
    }

    public void Add(GameObject cell) =>
      _stack.Add(cell);

    public void Remove(GameObject cell) =>
      _stack.Remove(cell);

    public void SetActive(bool active) =>
      _stack.ForEach(go => go.SetActive(active));

    public Color GetLastCellColor() =>
      _stack.Last().GetComponent<Cell>().Color;

    public void Clear() =>
      _stack.Clear();
  }
}