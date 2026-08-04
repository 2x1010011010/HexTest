using UnityEngine;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  public class HexGridSettings
  {
    public GridType GridType = GridType.Rectangular;

    public int Width = 3;
    public int Height = 5;
    public int Radius = 3;

    public float Spacing = 13f;
    public bool AutoRotate;

    public GameObject HexPrefab;
  }
}