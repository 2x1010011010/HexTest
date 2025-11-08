using System.Collections.Generic;
using HexaSortTest.CodeBase.GameLogic.Cells;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.GridLogic
{
  public class HexGrid : MonoBehaviour
  {
    [SerializeField] private List<Cell> _cells = new();

    public List<Cell> Cells => _cells;

    public void Initialize()
    {
      var cells = GetComponentsInChildren<Cell>();
      foreach (var cell in cells)
        _cells.Add(cell);
    }
  }
}
