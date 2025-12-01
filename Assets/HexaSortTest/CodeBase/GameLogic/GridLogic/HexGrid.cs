using System.Collections.Generic;
using System.Linq;
using HexaSortTest.CodeBase.GameLogic.Cells;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.GridLogic
{
  public class HexGrid : MonoBehaviour
  {
    [SerializeField] private List<Cell> _cells;

    public List<Cell> Cells => _cells;

    public void Initialize() =>
      _cells = GetComponentsInChildren<Cell>().ToList();
  }
}