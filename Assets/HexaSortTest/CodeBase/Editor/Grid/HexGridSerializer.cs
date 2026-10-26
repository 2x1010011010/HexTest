using System;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using UnityEditor;
using UnityEngine;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  public static class HexGridSerializer
  {
    private const string GridLayerName = "Grid";
    private const string DefaultLayerName = "Default";

    public static void SaveAsPrefab(HexGridData data, HexGridSettings settings)
    {
      if (settings?.HexPrefab == null || data == null || data.IsEmpty)
      {
        Debug.LogWarning("[HexGridSerializer] Nothing to save — missing hex prefab or empty grid.");
        return;
      }

      int gridLayer = LayerMask.NameToLayer(GridLayerName);
      int defaultLayer = LayerMask.NameToLayer(DefaultLayerName);
      bool hasActiveCells = data.Cells.Exists(cell =>
        cell.State != CellState.Disabled && cell.CoverType == CellCoverType.Normal);
      if (hasActiveCells && gridLayer < 0)
      {
        Debug.LogError($"[HexGridSerializer] Layer '{GridLayerName}' is missing. Add it in Project Settings > Tags and Layers before saving the grid.");
        return;
      }

      string defaultName = "HexGrid_" + DateTime.Now.ToString("HHmmss");
      string path = EditorUtility.SaveFilePanelInProject(
        "Save Grid as Prefab", defaultName, "prefab", "Enter prefab name");

      if (string.IsNullOrEmpty(path))
        return;

      GameObject root = BuildGridRoot(data, settings, gridLayer, defaultLayer);

      PrefabUtility.SaveAsPrefabAsset(root, path);
      UnityEngine.Object.DestroyImmediate(root);
      AssetDatabase.Refresh();

      EditorUtility.DisplayDialog("Grid Saved", "Hex grid saved successfully!", "OK");
    }

    private static GameObject BuildGridRoot(HexGridData data, HexGridSettings settings, int gridLayer, int defaultLayer)
    {
      var root = new GameObject("HexGrid");
      var grid = root.AddComponent<HexGrid>();
      var observer = root.AddComponent<GridObserver>();
      observer.Init(grid);

      foreach (var cell in data.Cells)
      {
        if (cell.State == CellState.Disabled)
          continue;

        SpawnCell(cell, settings, root.transform, gridLayer, defaultLayer);
      }

      grid.Initialize();
      return root;
    }

    private static void SpawnCell(HexCellData cellData, HexGridSettings settings, Transform parent, int gridLayer, int defaultLayer)
    {
      GameObject hexInstance = UnityEngine.Object.Instantiate(settings.HexPrefab, parent, true);
      settings.ApplyModelTransform(hexInstance.transform, cellData.Position);
      int cellLayer = cellData.CoverType == CellCoverType.Normal ? gridLayer : defaultLayer;
      SetLayerRecursively(hexInstance, cellLayer);

      var cellComponent = hexInstance.GetComponent<Cell>();
      if (cellComponent == null)
      {
        Debug.LogError($"[HexGridSerializer] Hex prefab '{settings.HexPrefab.name}' has no Cell component.");
        return;
      }

      cellComponent.SetSpawner(false);
      cellComponent.SetEmpty(true);

      GameObject coverVisual = HexCellCoverBuilder.Create(cellData, settings, parent);
      if (coverVisual != null)
        SetLayerRecursively(coverVisual, defaultLayer);
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

    private static void SetLayerRecursively(GameObject root, int layer)
    {
      // Child colliders must follow the authored cell's interaction layer too.
      foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
      {
        child.gameObject.layer = layer;
        if (PrefabUtility.IsPartOfPrefabInstance(child.gameObject))
          PrefabUtility.RecordPrefabInstancePropertyModifications(child.gameObject);
      }
    }
  }
}
