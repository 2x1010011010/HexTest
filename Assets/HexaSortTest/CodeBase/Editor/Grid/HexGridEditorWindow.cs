using HexaSortTest.CodeBase.Infrastructure.Services.AssetManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  public class HexGridEditorWindow : EditorWindow
  {
    private HexGridSettings _settings;
    private HexGridData _data;
    private HexGridPreview _preview;
    private HexGridEditorView _view;

    private IMGUIContainer _previewContainer;

    [MenuItem("Tools/Grid/Hex Grid Editor")]
    public static void ShowWindow()
    {
      var window = GetWindow<HexGridEditorWindow>("Hex Grid Editor");
      window.minSize = new Vector2(520, 420);
    }

    private void OnEnable()
    {
      _settings = new HexGridSettings
      {
        HexPrefab = Resources.Load<GameObject>(AssetPaths.CellPrefab)
      };

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
      _view.SettingsChanged -= RegenerateAndRepaint;
      _view.GenerateRequested -= RegenerateAndRepaint;
      _view.SaveRequested -= SaveGrid;
      _preview.CellClicked -= OnCellClicked;

      _preview.Dispose();
    }

    public void CreateGUI()
    {
      var splitView = new TwoPaneSplitView(0, 260, TwoPaneSplitViewOrientation.Horizontal);
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
      Regenerate();
      _previewContainer?.MarkDirtyRepaint();
    }

    private void Regenerate()
    {
      _data.Clear();
      _data.Cells.AddRange(HexGridGenerator.Generate(_settings));
    }

    private void OnCellClicked(HexCellData cell) =>
      HexCellContextMenu.Show(cell, () => _previewContainer?.MarkDirtyRepaint());

    private void SaveGrid() =>
      HexGridSerializer.SaveAsPrefab(_data, _settings);
  }
}
