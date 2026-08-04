using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  public class HexGridPreview : IDisposable
  {
    public event Action<HexCellData> CellClicked;

    private PreviewRenderUtility _previewRenderUtility;
    private readonly List<GameObject> _spawnedPreviewObjects = new();
    private readonly Dictionary<CellState, Material> _stateMaterials = new();

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

      DestroyTemporaryObjects();

      HandleMouseInput(rect, data);
    }

    #region Object Spawning

    private void SpawnTemporaryObjects(HexGridData data, HexGridSettings settings)
    {
      Quaternion rotationFix = settings.AutoRotate ? Quaternion.Euler(90f, 0f, 0f) : Quaternion.identity;

      foreach (var cell in data.Cells)
      {
        var go = Object.Instantiate(settings.HexPrefab);
        go.transform.position = cell.Position;
        go.transform.rotation = settings.HexPrefab.transform.rotation * rotationFix;
        go.transform.localScale = settings.HexPrefab.transform.localScale;

        ApplyCellColor(go, cell.State);

        _spawnedPreviewObjects.Add(go);
        _previewRenderUtility.AddSingleGO(go);
      }
    }

    private void ApplyCellColor(GameObject go, CellState state)
    {
      var renderer = go.GetComponentInChildren<Renderer>();
      if (renderer == null)
        return;

      renderer.sharedMaterial = GetOrCreateMaterial(state, renderer.sharedMaterial);
    }

    private Material GetOrCreateMaterial(CellState state, Material sourceMaterial)
    {
      if (_stateMaterials.TryGetValue(state, out var existing) && existing != null)
        return existing;

      var material = new Material(sourceMaterial) { color = ColorForState(state) };
      _stateMaterials[state] = material;
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
      Vector3 center = Vector3.zero;
      foreach (var cell in data.Cells)
        center += cell.Position;
      if (data.Cells.Count > 0)
        center /= data.Cells.Count;

      float camDistance = (settings.GridType == GridType.Circular
        ? settings.Radius * 1.5f
        : Mathf.Max(settings.Width, settings.Height)) * settings.Spacing;

      camDistance = Mathf.Max(camDistance, 1f);

      _previewRenderUtility.camera.transform.position = center + Vector3.up * (camDistance + 5f);
      _previewRenderUtility.camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
      _previewRenderUtility.camera.orthographicSize = camDistance * 0.4f;
    }

    #endregion

    #region Mouse Interaction

    private void HandleMouseInput(Rect rect, HexGridData data)
    {
      Event e = Event.current;
      if (e == null || e.type != EventType.MouseDown || e.button != 0 || !rect.Contains(e.mousePosition))
        return;

      var clicked = FindClosestCellByCameraProjection(e.mousePosition, rect, data);
      if (clicked == null)
        return;

      CellClicked?.Invoke(clicked);
      e.Use();
    }

    private HexCellData FindClosestCellByCameraProjection(Vector2 mousePos, Rect rect, HexGridData data)
    {
      if (data.Cells.Count == 0)
        return null;

      Camera cam = _previewRenderUtility.camera;
      float bestDist = float.MaxValue;
      HexCellData best = null;
      float clickRadiusPixels = Mathf.Max(12f, rect.width * 0.03f);

      foreach (var cell in data.Cells)
      {
        Vector3 viewPos = cam.WorldToViewportPoint(cell.Position);
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
