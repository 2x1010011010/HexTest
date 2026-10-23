using HexaSortTest.CodeBase.Infrastructure.Services.AssetManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  public class HexGridEditorWindow : EditorWindow
  {
    private string SettingsKey => $"{Application.dataPath}.HexGridEditor.Settings";

    [SerializeField] private HexGridSettings _settings;
    private HexGridData _data;
    private HexGridPreview _preview;
    private HexGridEditorView _view;

    private IMGUIContainer _previewContainer;
    private string _layoutKey;

    [MenuItem("Tools/Grid/Hex Grid Editor")]
    public static void ShowWindow()
    {
      var window = GetWindow<HexGridEditorWindow>("Hex Grid Editor");
      window.minSize = new Vector2(520, 420);
    }

    private void OnEnable()
    {
      if (_settings == null)
      {
        string json = EditorPrefs.GetString(SettingsKey, string.Empty);
        _settings = string.IsNullOrEmpty(json)
          ? new HexGridSettings { HexPrefab = Resources.Load<GameObject>(AssetPaths.CellPrefab) }
          : JsonUtility.FromJson<HexGridSettings>(json);

        string prefabGuid = EditorPrefs.GetString(SettingsKey + ".Prefab", string.Empty);
        string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
        if (!string.IsNullOrEmpty(prefabPath))
          _settings.HexPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        _settings.LockSprite = LoadSprite("LockSprite");
        _settings.BlockerSprite = LoadSprite("BlockerSprite");
      }

      _settings.EnsureCoverDefaults();
      _data = new HexGridData();
      _preview = new HexGridPreview();
      _view = new HexGridEditorView(_settings);

      _view.SettingsChanged += RegenerateAndRepaint;
      _view.GenerateRequested += RegenerateAndRepaint;
      _view.SaveRequested += SaveGrid;
      _preview.CellClicked += OnCellClicked;
    }

    private void OnDisable()
    {
      SaveSettings();
      _view.SettingsChanged -= RegenerateAndRepaint;
      _view.GenerateRequested -= RegenerateAndRepaint;
      _view.SaveRequested -= SaveGrid;
      _preview.CellClicked -= OnCellClicked;

      _preview.Dispose();
    }

    public void CreateGUI()
    {
      rootVisualElement.Clear();
      var splitView = new TwoPaneSplitView(0, 320, TwoPaneSplitViewOrientation.Horizontal);
      rootVisualElement.Add(splitView);

      splitView.Add(_view.Build());

      _previewContainer = new IMGUIContainer(DrawPreview) { style = { flexGrow = 1 } };
      splitView.Add(_previewContainer);

      Regenerate();
    }

    private void DrawPreview()
    {
      Rect rect = _previewContainer.contentRect;
      if (rect.width <= 0 || rect.height <= 0)
        return;

      var localRect = new Rect(0, 0, rect.width, rect.height);
      _preview.Draw(localRect, _data, _settings);
    }

    private void RegenerateAndRepaint()
    {
      SaveSettings();
      Regenerate();
      _previewContainer?.MarkDirtyRepaint();
    }

    private void Regenerate()
    {
      var generated = HexGridGenerator.Generate(_settings);
      // Preserve painted types/scores when model or cover appearance changes.
      // Index order is stable for a fixed grid shape.
      string layoutKey = $"{_settings.GridType}:{_settings.Width}:{_settings.Height}:{_settings.Radius}";
      if (layoutKey == _layoutKey && generated.Count == _data.Cells.Count)
        for (int i = 0; i < generated.Count; i++)
        {
          generated[i].State = _data.Cells[i].State;
          generated[i].CoverType = _data.Cells[i].CoverType;
          generated[i].RequiredScore = _data.Cells[i].RequiredScore;
        }
      _data.Clear();
      _data.Cells.AddRange(generated);
      _layoutKey = layoutKey;
    }

    private void OnCellClicked(HexCellData cell) =>
      HexCellContextMenu.Show(cell, _settings.DefaultRequiredScore, () => _previewContainer?.MarkDirtyRepaint());

    private void SaveSettings()
    {
      EditorPrefs.SetString(SettingsKey, JsonUtility.ToJson(_settings));
      EditorPrefs.SetString(SettingsKey + ".Prefab",
        AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_settings.HexPrefab)));
      SaveSprite("LockSprite", _settings.LockSprite);
      SaveSprite("BlockerSprite", _settings.BlockerSprite);
    }

    private void SaveSprite(string key, Sprite sprite)
    {
      EditorPrefs.SetString(SettingsKey + "." + key,
        sprite == null ? string.Empty : GlobalObjectId.GetGlobalObjectIdSlow(sprite).ToString());
    }

    private Sprite LoadSprite(string key)
    {
      string id = EditorPrefs.GetString(SettingsKey + "." + key, string.Empty);
      return GlobalObjectId.TryParse(id, out GlobalObjectId globalId)
        ? GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalId) as Sprite : null;
    }

    private void SaveGrid() =>
      HexGridSerializer.SaveAsPrefab(_data, _settings);
  }
}
