using System.Collections.Generic;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.StackLogic
{
  public class Stack : MonoBehaviour
  {
    public List<GameObject> _stack = new();
    private Transform _parent;
    
    public IReadOnlyList<GameObject> Tiles => _stack;
    public Transform Parent => _parent;
    
    public void SetParent(Transform parent)
    {
      _parent = parent;
    }

    public void Add(GameObject cell) => _stack.Add(cell);
    public void Remove(GameObject cell) => _stack.Remove(cell);
    public void SetActive(bool active) => _stack.ForEach(go => go.SetActive(active));
    public void Clear() => _stack.Clear();
  }
}