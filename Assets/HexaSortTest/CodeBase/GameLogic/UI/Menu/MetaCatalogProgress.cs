using System.Collections.Generic;
using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.Data;

namespace HexaSortTest.CodeBase.GameLogic.UI.Menu
{
  public static class MetaCatalogProgress
  {
    public static int FindInitialTile(MetaTileConfigsList catalog, List<MetaTileProgress> progress)
    {
      int selected = -1;
      if (catalog?.Tiles == null) return selected;
      for (int i = 0; i < catalog.Tiles.Count; i++)
      {
        if (!IsUnlocked(catalog, progress, i)) continue;
        selected = i;
        if (!IsUnlocked(catalog, progress, i + 1)) break;
      }
      return selected;
    }
    public static bool IsUnlocked(MetaTileConfigsList catalog, List<MetaTileProgress> progress, int index)
    {
      if (catalog?.Tiles == null || index < 0 || index >= catalog.Tiles.Count ||
          catalog.Tiles[index]?.TilePrefab == null) return false;
      if (index == 0) return true;
      var previous = catalog.Tiles[index - 1]?.TilePrefab;
      if (previous == null || !previous.ValidateLayout(out _)) return false;
      var saved = progress?.Find(p => p != null && p.TileId == previous.TileId);
      if (saved == null) return false;
      if (saved.UsesCellIds)
      {
        var cells = previous.Cells;
        return cells.Count > 0 && cells.TrueForAll(cell => saved.OpenedCellIds != null && saved.OpenedCellIds.Contains(cell.CellId));
      }
      return saved.UnlockedGroupsCount >= previous.GroupsCount;
    }
  }
}
