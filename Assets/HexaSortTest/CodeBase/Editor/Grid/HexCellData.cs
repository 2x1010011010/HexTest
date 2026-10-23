using UnityEngine;
using HexaSortTest.CodeBase.GameLogic.Cells;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  public class HexCellData
  {
    public Vector3 Position;
    public CellState State;
    public CellCoverType CoverType;
    public int RequiredScore;

    public HexCellData(Vector3 position, CellState state = CellState.Enabled)
    {
      Position = position;
      State = state;
    }
  }
}