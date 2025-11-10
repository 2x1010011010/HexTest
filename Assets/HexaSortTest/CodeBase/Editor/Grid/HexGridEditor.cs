using System.Collections.Generic;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using HexaSortTest.CodeBase.Infrastructure.Services.AssetManagement;
using UnityEditor;
using UnityEngine;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  public class HexGridEditor : EditorWindow
  {
    #region Enums and Structs

    private enum GridType
    {
      Rectangular,
      Circular
    }

    private enum CellState
    {
      Enabled,
      Disabled,
      SpawnPoint
    }

    private class HexCell
    {
      public HexCell(Vector3 position)
      {
        Position = position;
      }

      public Vector3 Position;
      public CellState State = CellState.Enabled;
    }

    #endregion

    #region Fields
    private GridType _gridType = GridType.Rectangular;
    private int _width = 3;
    private int _height = 5;
    private int _radius = 3;
    private Vector3 _hexSize;
    private float _spacing = 13f;
    private bool _autoRotate = false;

    private GameObject _hexPrefab;
    private readonly List<HexCell> _cells = new();

    private PreviewRenderUtility _previewRenderUtility;
    private Vector3 _lastCenter;
    private Rect _lastPreviewRect;
    #endregion

    #region Window

    [MenuItem("Tools/Grid/Hex Grid Editor")]
    public static void ShowWindow() => GetWindow<HexGridEditor>("Hex Grid Editor");

    #endregion

    #region Initialization and Cleanup

    private void OnEnable()
    {
      _hexPrefab = Resources.Load<GameObject>(AssetPaths.CellPrefab);
      _hexSize = _hexPrefab != null ? _hexPrefab.transform.localScale : Vector3.one;
      _previewRenderUtility ??= new PreviewRenderUtility();
      _previewRenderUtility.cameraFieldOfView = 80f;
      _previewRenderUtility.camera.farClipPlane = 200f;
      _previewRenderUtility.camera.nearClipPlane = 0.1f;
      GenerateGrid();
    }

    private void OnDisable() => _previewRenderUtility?.Cleanup();

    #endregion

    #region OnGUI Methods

    private void OnGUI()
    {
      EditorGUI.BeginChangeCheck();

      GUILayout.Label("HEX GRID SETTINGS", EditorStyles.boldLabel);
      _gridType = (GridType)EditorGUILayout.EnumPopup("Grid Type", _gridType);

      if (_gridType == GridType.Rectangular)
      {
        _width = EditorGUILayout.IntSlider("Width", _width, 1, 10);
        _height = EditorGUILayout.IntSlider("Height", _height, 1, 10);
      }
      else
      {
        _radius = EditorGUILayout.IntSlider("Radius", _radius, 1, 6);
      }

      EditorGUILayout.Space();
      GUILayout.Label("Hexagon Size:", EditorStyles.boldLabel);
      _spacing = EditorGUILayout.Slider("Spacing", _spacing, 0.8f, 20f);

      _autoRotate = EditorGUILayout.Toggle("Rotate Prefab (fix 90° X)", _autoRotate);
      EditorGUILayout.Space(10);

      _hexPrefab = (GameObject)EditorGUILayout.ObjectField("Hex Prefab", _hexPrefab, typeof(GameObject), false);

      if (GUILayout.Button("Generate Grid"))
        GenerateGrid();

      if (EditorGUI.EndChangeCheck())
      {
        GenerateGrid();
        Repaint();
      }

      if (_cells.Count > 0 && GUILayout.Button("Save Grid as Prefab"))
        SaveGridAsPrefab();

      Rect previewRect = GUILayoutUtility.GetRect(400, 450);
      DrawPreview(previewRect);

      HandleMouse(previewRect);
    }

    #endregion

    #region Grid Generation

    private void GenerateGrid()
    {
      _cells.Clear();
      if (_hexPrefab == null)
        return;

      if (_gridType == GridType.Rectangular)
      {
        int cols = _width;
        int rows = _height;

        for (int row = 0; row < rows; row++)
        {
          for (int col = 0; col < cols; col++)
          {
            Vector3 pos = OffsetToWorldForRect(col, row, _hexSize.x * _spacing);
            _cells.Add(new HexCell(pos));
          }
        }
      }
      else
      {
        for (int q = -_radius; q <= _radius; q++)
        {
          int r1 = Mathf.Max(-_radius, -q - _radius);
          int r2 = Mathf.Min(_radius, -q + _radius);
          for (int r = r1; r <= r2; r++)
          {
            Vector3 pos = AxialToWorld(q, r, _hexSize.x * _spacing);
            _cells.Add(new HexCell(pos));
          }
        }
      }

      CenterPositions();
    }

    private Vector3 OffsetToWorldForRect(int col, int row, float size)
    {
      float sqrt3 = Mathf.Sqrt(3f);
      float x = sqrt3 * size * (col + 0.5f * (row % 2));
      float z = 1.5f * size * row;
      return new Vector3(x, 0f, z);
    }

    private static Vector3 AxialToWorld(int q, int r, float size)
    {
      float x = size * (Mathf.Sqrt(3f) * q + Mathf.Sqrt(3f) / 2f * r);
      float z = size * (3f / 2f * r);
      return new Vector3(x, 0, z);
    }

    private void CenterPositions()
    {
      if (_cells.Count == 0) return;
      Vector3 center = Vector3.zero;
      foreach (var p in _cells) center += p.Position;
      center /= _cells.Count;

      for (int i = 0; i < _cells.Count; i++)
        _cells[i].Position -= center;
    }
#endregion

    #region Preview Rendering
    private void DrawPreview(Rect rect)
    {
      if (_previewRenderUtility == null) return;

      _lastPreviewRect = rect;

      _previewRenderUtility.BeginPreview(rect, GUIStyle.none);
      _previewRenderUtility.camera.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
      _previewRenderUtility.camera.clearFlags = CameraClearFlags.Color;

      Vector3 center = Vector3.zero;
      foreach (var c in _cells) center += c.Position;
      if (_cells.Count > 0) center /= _cells.Count;
      _lastCenter = center;

      List<GameObject> temp = new();
      Quaternion rotationFix = _autoRotate ? Quaternion.Euler(90f, 0f, 0f) : Quaternion.identity;

      foreach (var cell in _cells)
      {
        GameObject go = GameObject.Instantiate(_hexPrefab);
        go.transform.position = cell.Position;
        go.transform.rotation = _hexPrefab.transform.rotation * rotationFix;
        go.transform.localScale = _hexPrefab.transform.localScale;

        var renderer = go.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
          Color c = cell.State switch
          {
            CellState.Enabled => Color.white,
            CellState.Disabled => Color.gray,
            CellState.SpawnPoint => Color.green,
            _ => Color.white
          };
          Material mat = new Material(renderer.sharedMaterial);
          mat.color = c;
          renderer.sharedMaterial = mat;
        }

        temp.Add(go);
        _previewRenderUtility.AddSingleGO(go);
      }

      float camDist = (_gridType == GridType.Circular ? _radius * 1.5f: Mathf.Max(_width, _height)) * _spacing;
      _previewRenderUtility.camera.transform.position = center + Vector3.up * (camDist + 5f);
      _previewRenderUtility.camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
      _previewRenderUtility.camera.orthographic = true;
      _previewRenderUtility.camera.orthographicSize = camDist * 0.4f;
      _previewRenderUtility.camera.Render();

      Texture tex = _previewRenderUtility.EndPreview();
      GUI.DrawTexture(rect, tex, ScaleMode.StretchToFill, false);

      foreach (var obj in temp)
        if (obj != null)
          Object.DestroyImmediate(obj);
    }

    #endregion

    #region Cells Interaction

    private void HandleMouse(Rect previewRect)
    {
      Event e = Event.current;
      if (e.type == EventType.MouseDown && e.button == 0 && previewRect.Contains(e.mousePosition))
      {
        HexCell clicked = FindClosestCellByCameraProjection(e.mousePosition, previewRect);
        if (clicked != null)
        {
          ShowCellMenu(clicked);
          e.Use();
        }
      }
    }

    private HexCell FindClosestCellByCameraProjection(Vector2 mousePos, Rect rect)
    {
      if (_cells.Count == 0 || _previewRenderUtility == null) return null;

      Camera cam = _previewRenderUtility.camera;
      float bestDist = float.MaxValue;
      HexCell best = null;

      float clickRadiusPixels = Mathf.Max(12f, rect.width * 0.03f);

      foreach (var cell in _cells)
      {
        Vector3 viewPos = cam.WorldToViewportPoint(cell.Position);
        if (viewPos.z <= 0f) continue;

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

    private Vector3 WorldToPreview(Vector3 world, Rect rect)
    {
      Vector3 dir = world - _lastCenter;
      return new Vector3(rect.x + rect.width / 2 + dir.x * 3f, rect.y + rect.height / 2 - dir.z * 3f, 0);
    }

    private void ShowCellMenu(HexCell cell)
    {
      GenericMenu menu = new GenericMenu();
      menu.AddItem(new GUIContent("Enabled"), cell.State == CellState.Enabled, () => cell.State = CellState.Enabled);
      menu.AddItem(new GUIContent("Disabled"), cell.State == CellState.Disabled, () => cell.State = CellState.Disabled);
      menu.AddItem(new GUIContent("Spawn Point"), cell.State == CellState.SpawnPoint,
        () => cell.State = CellState.SpawnPoint);
      menu.ShowAsContext();
    }

    #endregion

    #region Save Grid as Prefab

    private void SaveGridAsPrefab()
    {
      if (_hexPrefab == null || _cells.Count == 0)
        return;

      string defaultName = "HexGrid_" + System.DateTime.Now.ToString("HHmmss");
      string path =
        EditorUtility.SaveFilePanelInProject("Save Grid as Prefab", defaultName, "prefab", "Enter prefab name");
      if (string.IsNullOrEmpty(path)) return;

      GameObject root = new GameObject("HexGrid");
      Quaternion rotationFix = _autoRotate ? Quaternion.Euler(90f, 0f, 0f) : Quaternion.identity;
      var grid = root.AddComponent<HexGrid>();
      root.AddComponent<GridRotator>();

      foreach (var cell in _cells)
      {
        if (cell.State == CellState.Disabled) continue;
        
        GameObject hex = Instantiate(_hexPrefab, root.transform, true);
        hex.transform.localPosition = cell.Position;
        hex.transform.localRotation = _hexPrefab.transform.rotation * rotationFix;
        hex.transform.localScale = _hexPrefab.transform.localScale;
        
        var cellComp = hex.GetComponent<Cell>();
        cellComp.SetEmpty(true);
        
        if (cell.State == CellState.SpawnPoint)
        {
          cellComp.SetSpawner(true);
          cellComp.SetEmpty(false);
        }
      }
      grid.Initialize();

      PrefabUtility.SaveAsPrefabAsset(root, path);
      DestroyImmediate(root);
      AssetDatabase.Refresh();
      EditorUtility.DisplayDialog("Grid Saved", "Hex grid saved successfully!", "OK");
    }

    #endregion
  }
}