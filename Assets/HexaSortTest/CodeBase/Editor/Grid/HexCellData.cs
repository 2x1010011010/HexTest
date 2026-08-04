using UnityEngine;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  public class HexCellData
  {
    public Vector3 Position;
    public CellState State;

    public HexCellData(Vector3 position, CellState state = CellState.Enabled)
    {
      Position = position;
      State = state;
    }
  }
}