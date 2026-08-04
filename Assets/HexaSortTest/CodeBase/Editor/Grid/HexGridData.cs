using System.Collections.Generic;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  public class HexGridData
  {
    public readonly List<HexCellData> Cells = new();

    public bool IsEmpty => Cells.Count == 0;

    public void Clear() => Cells.Clear();
  }
}