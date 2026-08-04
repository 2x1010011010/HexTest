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

      float size = settings.HexPrefab.transform.localScale.x * settings.Spacing;

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

    private static void GenerateRectangular(HexGridSettings settings, float size, List<HexCellData> cells)
    {
      for (int row = 0; row < settings.Height; row++)
        for (int col = 0; col < settings.Width; col++)
          cells.Add(new HexCellData(OffsetToWorldForRect(col, row, size)));
    }

    public static Vector3 OffsetToWorldForRect(int col, int row, float size)
    {
      float sqrt3 = Mathf.Sqrt(3f);
      float x = sqrt3 * size * (col + 0.5f * (row % 2));
      float z = 1.5f * size * row;
      return new Vector3(x, 0f, z);
    }

    #endregion

    #region Circular

    private static void GenerateCircular(HexGridSettings settings, float size, List<HexCellData> cells)
    {
      int radius = settings.Radius;

      for (int q = -radius; q <= radius; q++)
      {
        int r1 = Mathf.Max(-radius, -q - radius);
        int r2 = Mathf.Min(radius, -q + radius);

        for (int r = r1; r <= r2; r++)
          cells.Add(new HexCellData(AxialToWorld(q, r, size)));
      }
    }
    
    public static Vector3 AxialToWorld(int q, int r, float size)
    {
      float x = size * (Mathf.Sqrt(3f) * q + Mathf.Sqrt(3f) / 2f * r);
      float z = size * (3f / 2f * r);
      return new Vector3(x, 0f, z);
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
