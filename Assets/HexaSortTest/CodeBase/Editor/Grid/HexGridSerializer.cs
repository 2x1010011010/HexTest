using System;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using UnityEditor;
using UnityEngine;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  public static class HexGridSerializer
  {
    public static void SaveAsPrefab(HexGridData data, HexGridSettings settings)
    {
      if (settings?.HexPrefab == null || data == null || data.IsEmpty)
      {
        Debug.LogWarning("[HexGridSerializer] Nothing to save — missing hex prefab or empty grid.");
        return;
      }

      string defaultName = "HexGrid_" + DateTime.Now.ToString("HHmmss");
      string path = EditorUtility.SaveFilePanelInProject(
        "Save Grid as Prefab", defaultName, "prefab", "Enter prefab name");

      if (string.IsNullOrEmpty(path))
        return;

      GameObject root = BuildGridRoot(data, settings);

      PrefabUtility.SaveAsPrefabAsset(root, path);
      UnityEngine.Object.DestroyImmediate(root);
      AssetDatabase.Refresh();

      EditorUtility.DisplayDialog("Grid Saved", "Hex grid saved successfully!", "OK");
    }

    private static GameObject BuildGridRoot(HexGridData data, HexGridSettings settings)
    {
      var root = new GameObject("HexGrid");
      var grid = root.AddComponent<HexGrid>();
      var observer = root.AddComponent<GridObserver>();
      observer.Init(grid);

      foreach (var cell in data.Cells)
      {
        if (cell.State == CellState.Disabled)
          continue;

        SpawnCell(cell, settings, root.transform);
      }

      grid.Initialize();
      return root;
    }

    private static void SpawnCell(HexCellData cellData, HexGridSettings settings, Transform parent)
    {
      GameObject hexInstance = UnityEngine.Object.Instantiate(settings.HexPrefab, parent, true);
      settings.ApplyModelTransform(hexInstance.transform, cellData.Position);

      var cellComponent = hexInstance.GetComponent<Cell>();
      if (cellComponent == null)
      {
        Debug.LogError($"[HexGridSerializer] Hex prefab '{settings.HexPrefab.name}' has no Cell component.");
        return;
      }

      cellComponent.SetSpawner(false);
      cellComponent.SetEmpty(true);

      GameObject coverVisual = HexCellCoverBuilder.Create(cellData, settings, parent);
      var coverData = hexInstance.GetComponent<CellCoverData>();
      if (coverData == null)
        coverData = hexInstance.AddComponent<CellCoverData>();
      coverData.Initialize(cellData.CoverType, cellData.RequiredScore, cellData.RequiredHits, coverVisual);

      if (cellData.State == CellState.SpawnPoint)
      {
        cellComponent.SetSpawner(true);
        cellComponent.SetEmpty(false);
      }
    }
  }
}
