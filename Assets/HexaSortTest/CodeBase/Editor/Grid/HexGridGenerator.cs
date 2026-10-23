using System.Collections.Generic;
using UnityEngine;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  public static class HexGridGenerator
  {
    public static List<HexCellData> Generate(HexGridSettings settings)
    {
      var cells = new List<HexCellData>();

      if (settings == null || settings.HexPrefab == null)
        return cells;

      Vector2 size = settings.CellSize * Mathf.Max(0.0001f, settings.Spacing);

      switch (settings.GridType)
      {
        case GridType.Rectangular:
          GenerateRectangular(settings, size, cells);
          break;
        case GridType.Circular:
          GenerateCircular(settings, size, cells);
          break;
      }

      CenterPositions(cells);
      return cells;
    }

    #region Rectangular

    private static void GenerateRectangular(HexGridSettings settings, Vector2 size, List<HexCellData> cells)
    {
      for (int row = 0; row < settings.Height; row++)
        for (int col = 0; col < settings.Width; col++)
          cells.Add(new HexCellData(OffsetToWorldForRect(col, row, size, settings.FlatTop)));
    }

    public static Vector3 OffsetToWorldForRect(int col, int row, Vector2 size, bool flatTop)
    {
      return flatTop
        ? new Vector3(0.75f * size.x * col, 0f, size.y * (row + 0.5f * (col % 2)))
        : new Vector3(size.x * (col + 0.5f * (row % 2)), 0f, 0.75f * size.y * row);
    }

    #endregion

    #region Circular

    private static void GenerateCircular(HexGridSettings settings, Vector2 size, List<HexCellData> cells)
    {
      int radius = settings.Radius;

      for (int q = -radius; q <= radius; q++)
      {
        int r1 = Mathf.Max(-radius, -q - radius);
        int r2 = Mathf.Min(radius, -q + radius);

        for (int r = r1; r <= r2; r++)
          cells.Add(new HexCellData(AxialToWorld(q, r, size, settings.FlatTop)));
      }
    }

    public static Vector3 AxialToWorld(int q, int r, Vector2 size, bool flatTop)
    {
      return flatTop
        ? new Vector3(0.75f * size.x * q, 0f, size.y * (r + 0.5f * q))
        : new Vector3(size.x * (q + 0.5f * r), 0f, 0.75f * size.y * r);
    }

    #endregion

    #region Centering

    private static void CenterPositions(List<HexCellData> cells)
    {
      if (cells.Count == 0)
        return;

      Vector3 center = Vector3.zero;
      foreach (var cell in cells)
        center += cell.Position;
      center /= cells.Count;

      foreach (var cell in cells)
        cell.Position -= center;
    }

    #endregion
  }
}
