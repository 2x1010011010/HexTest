using System.Collections.Generic;
using HexaSortTest.CodeBase.Infrastructure.Services.AssetManagement;
using UnityEditor;
using UnityEngine;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  public class HexGridEditor : EditorWindow
  {
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

    private GridType _gridType = GridType.Rectangular;
    private int _width = 3;
    private int _height = 5;
    private int _radius = 3;
    private Vector3 _hexSize = new Vector3(0.5f, 0.5f, 0.05f);
    private float _spacing = 6.5f;
    private bool _autoRotate = false;

    private GameObject _hexPrefab;
    private readonly List<HexCell> _cells = new();

    private PreviewRenderUtility _previewRenderUtility;
    private Vector3 _lastCenter;

    [MenuItem("Tools/Grid/Hex Grid Editor")]
    public static void ShowWindow() => GetWindow<HexGridEditor>("Hex Grid Editor");

    private void OnEnable()
    {
      _hexPrefab = Resources.Load<GameObject>(AssetPaths.CellPrefab);
      _previewRenderUtility ??= new PreviewRenderUtility();
      _previewRenderUtility.cameraFieldOfView = 80f;
      _previewRenderUtility.camera.farClipPlane = 200f;
      _previewRenderUtility.camera.nearClipPlane = 0.1f;
      GenerateGrid();
    }

    private void OnDisable() => _previewRenderUtility?.Cleanup();

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
      _hexSize.x = EditorGUILayout.Slider("Size X", _hexSize.x, 0.1f, 2f);
      _hexSize.y = EditorGUILayout.Slider("Size Y", _hexSize.y, 0.1f, 2f);
      _hexSize.z = EditorGUILayout.Slider("Size Z", _hexSize.z, 0.01f, 1f);
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

    private void CenterPositions()
    {
      if (_cells.Count == 0) return;
      Vector3 center = Vector3.zero;
      foreach (var p in _cells) center += p.Position;
      center /= _cells.Count;

      for (int i = 0; i < _cells.Count; i++)
        _cells[i].Position -= center;
    }

    private void DrawPreview(Rect rect)
    {
      if (_previewRenderUtility == null) return;

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
        go.transform.localScale = _hexPrefab.transform.localScale * _hexSize.x;
        
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
          renderer.sharedMaterial = new Material(renderer.sharedMaterial) { color = c };
        }

        temp.Add(go);
        _previewRenderUtility.AddSingleGO(go);
      }

      float camDist = (_gridType == GridType.Circular ? _radius * 1.5f : Mathf.Max(_width, _height)) * _spacing;
      _previewRenderUtility.camera.transform.position = center + Vector3.up * 20f;
      _previewRenderUtility.camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
      _previewRenderUtility.camera.orthographic = true;
      _previewRenderUtility.camera.orthographicSize = camDist * 0.8f;
      _previewRenderUtility.camera.Render();

      Texture tex = _previewRenderUtility.EndPreview();
      GUI.DrawTexture(rect, tex, ScaleMode.StretchToFill, false);

      foreach (var obj in temp)
        if (obj != null)
          Object.DestroyImmediate(obj);
    }

    private void HandleMouse(Rect previewRect)
    {
      Event e = Event.current;
      if (e.type == EventType.MouseDown && e.button == 0 && previewRect.Contains(e.mousePosition))
      {
        Vector2 mousePos = e.mousePosition;
        HexCell clicked = FindClosestCell(mousePos, previewRect);
        if (clicked != null)
          ShowCellMenu(clicked);
      }
    }

    private HexCell FindClosestCell(Vector2 mousePos, Rect rect)
    {
      if (_cells.Count == 0) return null;
      HexCell closest = null;
      float minDist = float.MaxValue;
      foreach (var cell in _cells)
      {
        Vector3 screen = WorldToPreview(cell.Position, rect);
        float dist = Vector2.Distance(mousePos, new Vector2(screen.x, screen.y));
        if (dist < minDist && dist < 25f) 
        {
          minDist = dist;
          closest = cell;
        }
      }

      return closest;
    }

    private Vector3 WorldToPreview(Vector3 world, Rect rect)
    {
      Vector3 camPos = _previewRenderUtility.camera.transform.position;
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

    private static Vector3 AxialToWorld(int q, int r, float size)
    {
      float x = size * (Mathf.Sqrt(3f) * q + Mathf.Sqrt(3f) / 2f * r);
      float z = size * (3f / 2f * r);
      return new Vector3(x, 0, z);
    }

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

      foreach (var cell in _cells)
      {
        GameObject hex = (GameObject)PrefabUtility.InstantiatePrefab(_hexPrefab);
        hex.transform.SetParent(root.transform);
        hex.transform.localPosition = cell.Position;
        hex.transform.localRotation = rotationFix;
        hex.transform.localScale = _hexPrefab.transform.localScale * _hexSize.x;

        if (cell.State == CellState.Disabled)
          hex.SetActive(false);
      }

      PrefabUtility.SaveAsPrefabAsset(root, path);
      DestroyImmediate(root);
      AssetDatabase.Refresh();
      EditorUtility.DisplayDialog("Grid Saved", "Hex grid saved successfully!", "OK");
    }
  }
}