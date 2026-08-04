using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  public class HexGridEditorView
  {
    public event Action SettingsChanged;
    public event Action GenerateRequested;
    public event Action SaveRequested;

    private readonly HexGridSettings _settings;

    private VisualElement _root;
    private VisualElement _rectangularFields;
    private VisualElement _circularFields;

    public HexGridEditorView(HexGridSettings settings) =>
      _settings = settings;

    public VisualElement Build()
    {
      _root = new VisualElement
      {
        style = { paddingLeft = 8, paddingRight = 8, paddingTop = 8, paddingBottom = 8 }
      };

      _root.Add(Header("HEX GRID SETTINGS"));

      BuildGridTypeField();
      BuildRectangularFields();
      BuildCircularFields();

      _root.Add(Header("Hexagon Size"));
      BuildSpacingField();
      BuildAutoRotateField();
      BuildPrefabField();

      BuildButtons();

      UpdateGridTypeVisibility();

      return _root;
    }

    #region Field Builders

    private void BuildGridTypeField()
    {
      var field = new EnumField("Grid Type", _settings.GridType);
      field.RegisterValueChangedCallback(evt =>
      {
        _settings.GridType = (GridType)evt.newValue;
        UpdateGridTypeVisibility();
        RaiseSettingsChanged();
      });
      _root.Add(field);
    }

    private void BuildRectangularFields()
    {
      _rectangularFields = new VisualElement();

      var width = new IntegerField("Width") { value = _settings.Width };
      width.RegisterValueChangedCallback(evt =>
      {
        _settings.Width = Mathf.Max(1, evt.newValue);
        RaiseSettingsChanged();
      });

      var height = new IntegerField("Height") { value = _settings.Height };
      height.RegisterValueChangedCallback(evt =>
      {
        _settings.Height = Mathf.Max(1, evt.newValue);
        RaiseSettingsChanged();
      });

      _rectangularFields.Add(width);
      _rectangularFields.Add(height);
      _root.Add(_rectangularFields);
    }

    private void BuildCircularFields()
    {
      _circularFields = new VisualElement();

      var radius = new IntegerField("Radius") { value = _settings.Radius };
      radius.RegisterValueChangedCallback(evt =>
      {
        _settings.Radius = Mathf.Max(1, evt.newValue);
        RaiseSettingsChanged();
      });

      _circularFields.Add(radius);
      _root.Add(_circularFields);
    }

    private void BuildSpacingField()
    {
      var slider = new Slider("Spacing", 0.8f, 20f) { value = _settings.Spacing };
      slider.RegisterValueChangedCallback(evt =>
      {
        _settings.Spacing = evt.newValue;
        RaiseSettingsChanged();
      });
      _root.Add(slider);
    }

    private void BuildAutoRotateField()
    {
      var toggle = new Toggle("Rotate Prefab (fix 90° X)") { value = _settings.AutoRotate };
      toggle.RegisterValueChangedCallback(evt =>
      {
        _settings.AutoRotate = evt.newValue;
        RaiseSettingsChanged();
      });
      _root.Add(toggle);
    }

    private void BuildPrefabField()
    {
      var field = new ObjectField("Hex Prefab")
      {
        objectType = typeof(GameObject),
        value = _settings.HexPrefab
      };
      field.RegisterValueChangedCallback(evt =>
      {
        _settings.HexPrefab = evt.newValue as GameObject;
        RaiseSettingsChanged();
      });
      _root.Add(field);
    }

    private void BuildButtons()
    {
      var spacer = new VisualElement { style = { marginTop = 10 } };
      _root.Add(spacer);

      var generateButton = new Button(() => GenerateRequested?.Invoke()) { text = "Generate Grid" };
      _root.Add(generateButton);

      var saveButton = new Button(() => SaveRequested?.Invoke())
      {
        text = "Save Grid as Prefab",
        style = { marginTop = 4 }
      };
      _root.Add(saveButton);
    }

    #endregion

    private static Label Header(string text) => new(text)
    {
      style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 6, marginBottom = 4 }
    };

    private void UpdateGridTypeVisibility()
    {
      bool isRectangular = _settings.GridType == GridType.Rectangular;
      _rectangularFields.style.display = isRectangular ? DisplayStyle.Flex : DisplayStyle.None;
      _circularFields.style.display = isRectangular ? DisplayStyle.None : DisplayStyle.Flex;
    }

    private void RaiseSettingsChanged() => SettingsChanged?.Invoke();
  }
}
