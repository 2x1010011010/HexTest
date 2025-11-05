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

    private GridType _gridType = GridType.Rectangular;
    private int _width = 3;
    private int _height = 5;
    private int _radius = 3;
    private Vector3 _hexSize = new Vector3(0.5f, 0.5f, 0.05f);
    private float _spacing = 13f;
    private bool _autoRotate = false;

    private GameObject _hexPrefab;
    private readonly List<Vector3> _hexPositions = new();

    private PreviewRenderUtility _previewRenderUtility;
    private Vector3 _lastCenter;

    [MenuItem("Tools/Grid/Hex Grid Editor")]
    public static void ShowWindow()
    {
      var window = GetWindow<HexGridEditor>("Hex Grid Editor");
      window.Show();
    }

    private void OnEnable()
    {
      _hexPrefab = Resources.Load<GameObject>(AssetPaths.CellPrefab);

      _previewRenderUtility ??= new PreviewRenderUtility();
      _previewRenderUtility.cameraFieldOfView = 80f;
      _previewRenderUtility.camera.farClipPlane = 200f;
      _previewRenderUtility.camera.nearClipPlane = 0.1f;

      GenerateGrid();
    }

    private void OnDisable()
    {
      _previewRenderUtility?.Cleanup();
    }

    private void OnGUI()
    {
      EditorGUI.BeginChangeCheck();

      GUILayout.Label("HEX GRID SETTINGS", EditorStyles.boldLabel);
      _gridType = (GridType)EditorGUILayout.EnumPopup("Grid Type", _gridType);

      if (_gridType == GridType.Rectangular)
      {
        _width = EditorGUILayout.IntSlider("Width", _width, 1, 3);
        _height = EditorGUILayout.IntSlider("Height", _height, 1, 5);
      }
      else
      {
        _radius = EditorGUILayout.IntSlider("Radius", _radius, 1, 4);
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
      {
        GenerateGrid();
      }

      if (EditorGUI.EndChangeCheck())
      {
        GenerateGrid();
        Repaint();
      }

      if (_hexPositions.Count > 0 && GUILayout.Button("Save Grid as Prefab"))
      {
        SaveGridAsPrefab();
      }

      Rect previewRect = GUILayoutUtility.GetRect(400, 450);
      DrawPreview(previewRect);
    }

    private void GenerateGrid()
    {
      _hexPositions.Clear();
      if (_hexPrefab == null)
        return;

      if (_gridType == GridType.Rectangular)
      {
        for (int q = 0; q < _width; q++)
        {
          for (int r = 0; r < _height; r++)
          {
            Vector3 pos = AxialToWorld(q - _width / 2, r - _height / 2, _hexSize.x * _spacing);
            _hexPositions.Add(pos);
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
            _hexPositions.Add(pos);
          }
        }
      }
    }

    private void DrawPreview(Rect rect)
    {
      if (_previewRenderUtility == null) return;

      _previewRenderUtility.BeginPreview(rect, GUIStyle.none);
      _previewRenderUtility.camera.backgroundColor = new Color(0.18f, 0.18f, 0.18f);
      _previewRenderUtility.camera.clearFlags = CameraClearFlags.Color;

      Vector3 center = Vector3.zero;
      foreach (var pos in _hexPositions) center += pos;
      if (_hexPositions.Count > 0) center /= _hexPositions.Count;
      _lastCenter = center;

      List<GameObject> tempObjects = new();
      Quaternion rotationFix = _autoRotate ? Quaternion.Euler(90f, 0f, 0f) : Quaternion.identity;

      if (_hexPrefab != null)
      {
        foreach (var pos in _hexPositions)
        {
          GameObject instance = GameObject.Instantiate(_hexPrefab);
          instance.transform.position = pos;
          instance.transform.rotation = _hexPrefab.transform.rotation * rotationFix;
          instance.transform.localScale = _hexPrefab.transform.localScale;
          tempObjects.Add(instance);
          _previewRenderUtility.AddSingleGO(instance);
        }
      }

      float camDistance = (_gridType == GridType.Circular ? _radius * 2f: Mathf.Max(_width, _height)) * 8f;
      Vector3 camPos = center + Vector3.up * camDistance;
      _previewRenderUtility.camera.transform.position = camPos;
      _previewRenderUtility.camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
      _previewRenderUtility.camera.orthographic = true;
      _previewRenderUtility.camera.orthographicSize = camDistance;
      _previewRenderUtility.camera.nearClipPlane = 0.1f;
      _previewRenderUtility.camera.farClipPlane = 500f;

      _previewRenderUtility.camera.Render();

      Texture tex = _previewRenderUtility.EndPreview();
      GUI.DrawTexture(rect, tex, ScaleMode.StretchToFill, false);

      foreach (var obj in tempObjects)
        if (obj != null)
          Object.DestroyImmediate(obj);
    }

    private static Vector3 AxialToWorld(int q, int r, float size)
    {
      float x = size * (Mathf.Sqrt(3f) * q + Mathf.Sqrt(3f) / 2f * r);
      float z = size * (3f / 2f * r);
      return new Vector3(x, 0, z);
    }

    private void SaveGridAsPrefab()
    {
      if (_hexPrefab == null || _hexPositions.Count == 0)
        return;

      string defaultName = "HexGrid_" + System.DateTime.Now.ToString("HHmmss");
      string path =
        EditorUtility.SaveFilePanelInProject("Save Grid as Prefab", defaultName, "prefab", "Enter prefab name");
      if (string.IsNullOrEmpty(path)) return;

      GameObject root = new GameObject("HexGrid");
      Quaternion rotationFix = _autoRotate ? Quaternion.Euler(90f, 0f, 0f) : Quaternion.identity;

      foreach (var pos in _hexPositions)
      {
        GameObject hex = (GameObject)PrefabUtility.InstantiatePrefab(_hexPrefab);
        hex.transform.SetParent(root.transform);
        hex.transform.localPosition = pos;
        hex.transform.localRotation = rotationFix;
        hex.transform.localScale = _hexPrefab.transform.localScale;
      }

      PrefabUtility.SaveAsPrefabAsset(root, path);
      DestroyImmediate(root);
      AssetDatabase.Refresh();
      EditorUtility.DisplayDialog("Grid Saved", "Hex grid saved successfully!", "OK");
    }
  }
}