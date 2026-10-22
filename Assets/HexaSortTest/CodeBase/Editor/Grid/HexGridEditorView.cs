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
      _root = new ScrollView
      {
        style = { paddingLeft = 8, paddingRight = 8, paddingTop = 8, paddingBottom = 8 }
      };

      _root.Add(Header("HEX GRID SETTINGS"));

      BuildGridTypeField();
      BuildRectangularFields();
      BuildCircularFields();

      _root.Add(Header("MODEL SETTINGS"));
      BuildPrefabField();
      BuildModelFields();
      BuildSpacingField();

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
      var field = new FloatField("Spacing Multiplier") { value = _settings.Spacing, isDelayed = true };
      field.tooltip = "1 = touching hexagons; greater than 1 adds a gap.";
      field.RegisterValueChangedCallback(evt =>
      {
        _settings.Spacing = Mathf.Max(0.0001f, evt.newValue);
        field.SetValueWithoutNotify(_settings.Spacing);
        RaiseSettingsChanged();
      });
      _root.Add(field);
    }

    private void BuildModelFields()
    {
      var manualSize = new Vector2Field("Footprint X / Z") { value = _settings.ModelSize, isDelayed = true };
      manualSize.tooltip = "Final dimensions in grid space, after rotation and scale. Used when automatic size is off.";
      manualSize.SetEnabled(!_settings.AutomaticModelSize);
      manualSize.RegisterValueChangedCallback(evt =>
      {
        _settings.ModelSize = new Vector2(Mathf.Max(0.0001f, evt.newValue.x), Mathf.Max(0.0001f, evt.newValue.y));
        manualSize.SetValueWithoutNotify(_settings.ModelSize);
        RaiseSettingsChanged();
      });

      var automaticSize = new Toggle("Size From Model") { value = _settings.AutomaticModelSize };
      automaticSize.tooltip = "Measure visible renderer bounds, including child transforms, model rotation and scale.";
      automaticSize.RegisterValueChangedCallback(evt =>
      {
        _settings.AutomaticModelSize = evt.newValue;
        manualSize.SetEnabled(!evt.newValue);
        RaiseSettingsChanged();
      });
      _root.Add(automaticSize);
      _root.Add(manualSize);

      var scale = new Vector3Field("Model Scale") { value = _settings.ModelScale, isDelayed = true };
      scale.RegisterValueChangedCallback(evt =>
      {
        _settings.ModelScale = evt.newValue;
        RaiseSettingsChanged();
      });
      _root.Add(scale);

      var rotation = new Vector3Field("Model Rotation") { value = _settings.ModelRotation, isDelayed = true };
      rotation.tooltip = "Additional rotation in degrees. For XY models, try X = 90.";
      rotation.RegisterValueChangedCallback(evt =>
      {
        _settings.ModelRotation = evt.newValue;
        RaiseSettingsChanged();
      });
      _root.Add(rotation);

      var offset = new Vector3Field("Model Offset") { value = _settings.ModelOffset, isDelayed = true };
      offset.RegisterValueChangedCallback(evt =>
      {
        _settings.ModelOffset = evt.newValue;
        RaiseSettingsChanged();
      });
      _root.Add(offset);

      var flatTop = new Toggle("Flat Top Hexagons") { value = _settings.FlatTop };
      flatTop.tooltip = "Enable for a horizontal top edge; disable for a vertex pointing along Z.";
      flatTop.RegisterValueChangedCallback(evt =>
      {
        _settings.FlatTop = evt.newValue;
        RaiseSettingsChanged();
      });
      _root.Add(flatTop);

      var measuredSize = new Label();
      var warning = new HelpBox("No visible renderer bounds found. Manual footprint is used.", HelpBoxMessageType.Warning);
      void RefreshSize()
      {
        Vector2 size = _settings.CellSize;
        measuredSize.text = $"Cell footprint: X {size.x:G4}, Z {size.y:G4}";
        warning.style.display = _settings.AutomaticModelSize && !_settings.TryGetModelBounds(out _)
          ? DisplayStyle.Flex : DisplayStyle.None;
      }
      SettingsChanged += RefreshSize;
      RefreshSize();
      _root.Add(measuredSize);
      _root.Add(warning);
    }

    private void BuildPrefabField()
    {
      var field = new ObjectField("Hex Prefab")
      {
        objectType = typeof(GameObject),
        allowSceneObjects = false,
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
