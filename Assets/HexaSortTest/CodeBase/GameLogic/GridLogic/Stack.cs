using System.Collections.Generic;
using HexaSortTest.CodeBase.GameLogic.Cells;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.GridLogic
{
  public class Stack : MonoBehaviour
  {
    private readonly List<Cell> _stack = new();

    public void Add(Cell cell) => _stack.Add(cell);
    public void Remove(Cell cell) => _stack.Remove(cell);
    public void Clear() => _stack.Clear();
  }
}