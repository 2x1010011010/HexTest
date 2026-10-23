using System;
using System.Collections.Generic;
using UnityEditor;
using HexaSortTest.CodeBase.GameLogic.Cells;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  public class HexGridPreview : IDisposable
  {
    public event Action<HexCellData> CellClicked;

    private PreviewRenderUtility _previewRenderUtility;
    private readonly List<GameObject> _spawnedPreviewObjects = new();
    private readonly Dictionary<(CellState, Material), Material> _stateMaterials = new();

    public HexGridPreview()
    {
      _previewRenderUtility = new PreviewRenderUtility();
      _previewRenderUtility.cameraFieldOfView = 80f;
      _previewRenderUtility.camera.farClipPlane = 200f;
      _previewRenderUtility.camera.nearClipPlane = 0.1f;
      _previewRenderUtility.camera.orthographic = true;
    }

    public void Draw(Rect rect, HexGridData data, HexGridSettings settings)
    {
      if (_previewRenderUtility == null || data == null || settings?.HexPrefab == null || rect.width <= 0 || rect.height <= 0)
        return;

      _previewRenderUtility.BeginPreview(rect, GUIStyle.none);
      _previewRenderUtility.camera.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
      _previewRenderUtility.camera.clearFlags = CameraClearFlags.Color;

      SpawnTemporaryObjects(data, settings);
      FrameCamera(rect, data, settings);

      _previewRenderUtility.camera.Render();
      Texture texture = _previewRenderUtility.EndPreview();
      GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, false);

      DrawCoverLabels(rect, data, settings);
      DestroyTemporaryObjects();

      HandleMouseInput(rect, data, settings);
    }

    #region Object Spawning

    private void SpawnTemporaryObjects(HexGridData data, HexGridSettings settings)
    {
      foreach (var cell in data.Cells)
      {
        var go = Object.Instantiate(settings.HexPrefab);
        settings.ApplyModelTransform(go.transform, cell.Position);

        ApplyCellColor(go, cell.State);

        _spawnedPreviewObjects.Add(go);
        _previewRenderUtility.AddSingleGO(go);

        GameObject cover = HexCellCoverBuilder.Create(cell, settings, null, true);
        if (cover != null)
        {
          _spawnedPreviewObjects.Add(cover);
          _previewRenderUtility.AddSingleGO(cover);
        }
      }
    }

    private void DrawCoverLabels(Rect rect, HexGridData data, HexGridSettings settings)
    {
      Vector3 center = settings.TryGetModelBounds(out Bounds bounds) ? bounds.center : settings.ModelOffset;
      foreach (var cell in data.Cells)
      {
        if (cell.State == CellState.Disabled || cell.CoverType == CellCoverType.Normal)
          continue;
        Vector3 viewport = _previewRenderUtility.camera.WorldToViewportPoint(cell.Position + center);
        // A supplied lock prefab owns its TMP label. Until it exists, show an editor-only number above the lock.
        if (cell.CoverType == CellCoverType.ScoreLock
          && HexCellCoverBuilder.FindScoreText(settings.LockVisualPrefab, settings.LockScoreTextPath) != null)
          continue;
        string label = cell.CoverType == CellCoverType.ScoreLock
          ? cell.RequiredScore.ToString() : $"HITS: {cell.RequiredHits}";
        Vector2 size = EditorStyles.helpBox.CalcSize(new GUIContent(label));
        float halfCellPixels = settings.CellSize.y * rect.height / (4f * _previewRenderUtility.camera.orthographicSize);
        GUI.Label(new Rect(rect.x + viewport.x * rect.width - size.x / 2f,
          rect.y + (1f - viewport.y) * rect.height - halfCellPixels - size.y,
          size.x, size.y), label, EditorStyles.helpBox);
      }
    }

    private void ApplyCellColor(GameObject go, CellState state)
    {
      foreach (var renderer in go.GetComponentsInChildren<Renderer>())
      {
        Material[] materials = renderer.sharedMaterials;
        for (int i = 0; i < materials.Length; i++)
          if (materials[i] != null)
            materials[i] = GetOrCreateMaterial(state, materials[i]);
        renderer.sharedMaterials = materials;
      }
    }

    private Material GetOrCreateMaterial(CellState state, Material sourceMaterial)
    {
      var key = (state, sourceMaterial);
      if (_stateMaterials.TryGetValue(key, out var existing) && existing != null)
        return existing;

      var material = new Material(sourceMaterial) { color = ColorForState(state) };
      _stateMaterials[key] = material;
      return material;
    }

    public static Color ColorForState(CellState state) => state switch
    {
      CellState.Enabled => Color.white,
      CellState.Disabled => Color.gray,
      CellState.SpawnPoint => Color.green,
      _ => Color.white
    };

    private void DestroyTemporaryObjects()
    {
      foreach (var go in _spawnedPreviewObjects)
        if (go != null)
          Object.DestroyImmediate(go);

      _spawnedPreviewObjects.Clear();
    }

    #endregion

    #region Camera Framing

    private void FrameCamera(Rect rect, HexGridData data, HexGridSettings settings)
    {
      Bounds modelBounds;
      if (!settings.TryGetModelBounds(out modelBounds))
        modelBounds = new Bounds(settings.ModelOffset,
          new Vector3(settings.CellSize.x, 0f, settings.CellSize.y));

      Bounds gridBounds = modelBounds;
      bool first = true;
      foreach (var cell in data.Cells)
      {
        var cellBounds = new Bounds(cell.Position + modelBounds.center, modelBounds.size);
        if (first)
        {
          gridBounds = cellBounds;
          first = false;
        }
        else
          gridBounds.Encapsulate(cellBounds);
      }

      foreach (var go in _spawnedPreviewObjects)
        foreach (var renderer in go.GetComponentsInChildren<Renderer>())
          if (renderer.enabled && !(renderer is ParticleSystemRenderer) && !(renderer is TrailRenderer))
            gridBounds.Encapsulate(renderer.bounds);

      Camera camera = _previewRenderUtility.camera;
      float aspect = Mathf.Max(0.0001f, rect.width / rect.height);
      float halfSize = Mathf.Max(gridBounds.extents.z, gridBounds.extents.x / aspect);
      camera.orthographicSize = Mathf.Max(0.0001f, halfSize * 1.1f);
      float distance = gridBounds.extents.y + camera.orthographicSize + camera.nearClipPlane;
      camera.farClipPlane = distance + gridBounds.extents.y + camera.orthographicSize;
      camera.transform.position = gridBounds.center + Vector3.up * distance;
      camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    #endregion

    #region Mouse Interaction

    private void HandleMouseInput(Rect rect, HexGridData data, HexGridSettings settings)
    {
      Event e = Event.current;
      if (e == null || e.type != EventType.MouseDown || e.button != 0 || !rect.Contains(e.mousePosition))
        return;

      var clicked = FindClosestCellByCameraProjection(e.mousePosition, rect, data, settings);
      if (clicked == null)
        return;

      CellClicked?.Invoke(clicked);
      e.Use();
    }

    private HexCellData FindClosestCellByCameraProjection(Vector2 mousePos, Rect rect, HexGridData data, HexGridSettings settings)
    {
      if (data.Cells.Count == 0)
        return null;

      Camera cam = _previewRenderUtility.camera;
      float bestDist = float.MaxValue;
      HexCellData best = null;
      Vector2 size = settings.CellSize;
      float clickRadiusPixels = Mathf.Min(size.x, size.y) * 0.5f * rect.height / (2f * cam.orthographicSize);

      Vector3 modelCenter = settings.TryGetModelBounds(out Bounds bounds) ? bounds.center : settings.ModelOffset;
      foreach (var cell in data.Cells)
      {
        Vector3 viewPos = cam.WorldToViewportPoint(cell.Position + modelCenter);
        if (viewPos.z <= 0f)
          continue;

        float guiX = rect.x + viewPos.x * rect.width;
        float guiY = rect.y + (1f - viewPos.y) * rect.height;

        float dist = Vector2.Distance(mousePos, new Vector2(guiX, guiY));
        if (dist < bestDist)
        {
          bestDist = dist;
          best = cell;
        }
      }

      return bestDist <= clickRadiusPixels ? best : null;
    }

    #endregion

    public void Dispose()
    {
      DestroyTemporaryObjects();

      foreach (var material in _stateMaterials.Values)
        if (material != null)
          Object.DestroyImmediate(material);
      _stateMaterials.Clear();

      _previewRenderUtility?.Cleanup();
      _previewRenderUtility = null;
    }
  }
}
