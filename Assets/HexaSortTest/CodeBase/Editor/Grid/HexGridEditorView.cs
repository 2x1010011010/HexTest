using System;
using HexaSortTest.CodeBase.GameLogic.Cells;
using UnityEditor;
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
      BuildCoverFields();

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

    private void BuildCoverFields()
    {
      var lockBlock = new Foldout { text = "LOCK SETTINGS", value = true };
      lockBlock.style.marginTop = 8;
      _root.Add(lockBlock);
      AddPrefabField(lockBlock, _settings.LockVisualPrefab, prefab => _settings.LockVisualPrefab = prefab);
      var textPath = new TextField("Score Text Path") { value = _settings.LockScoreTextPath, isDelayed = true };
      textPath.tooltip = "Legacy TMP-only prefabs: optional child path. With ScoreLocker, its assigned Score Text is used instead.";
      textPath.RegisterValueChangedCallback(evt =>
      {
        _settings.LockScoreTextPath = evt.newValue;
        RaiseSettingsChanged();
      });
      lockBlock.Add(textPath);
      void RefreshTextPath()
      {
        textPath.SetEnabled(_settings.LockVisualPrefab == null
          || _settings.LockVisualPrefab.GetComponent<ScoreLocker>() == null);
      }
      SettingsChanged += RefreshTextPath;
      RefreshTextPath();
      lockBlock.Add(new HelpBox("Add ScoreLocker to the lock prefab root. Assign its Sprite Renderer, World Space Canvas, Score Text and optional Unlock Particles. Font and label position are configured in the prefab.", HelpBoxMessageType.Info));
      var textWarning = new HelpBox("No Score Text assigned in ScoreLocker (or no TMP at the legacy path). Check the prefab references; exported visuals need an assigned label.", HelpBoxMessageType.Warning);
      void RefreshTextWarning()
      {
        textWarning.style.display = _settings.LockVisualPrefab != null
          && HexCellCoverBuilder.FindScoreText(_settings.LockVisualPrefab, _settings.LockScoreTextPath) == null
          ? DisplayStyle.Flex : DisplayStyle.None;
      }
      SettingsChanged += RefreshTextWarning;
      RefreshTextWarning();
      lockBlock.Add(textWarning);
      var legacyLock = new Foldout { text = "Legacy Sprite Fallback", value = false };
      AddSpriteField(legacyLock, "Sprite", _settings.LockSprite, sprite => _settings.LockSprite = sprite);
      lockBlock.Add(legacyLock);
      AddCoverFloat(lockBlock, "Scale", _settings.LockScale, 0.0001f,
        value => _settings.LockScale = value,
        "Lock size relative to the cell footprint; preserves sprite aspect ratio.");
      AddCoverFloat(lockBlock, "Elevation", _settings.LockElevation, 0f,
        value => _settings.LockElevation = value,
        "Lock height above the model as a fraction of the smaller cell dimension.");

      var score = new IntegerField("Default Unlock Score") { value = _settings.DefaultRequiredScore, isDelayed = true };
      score.tooltip = "Initial score for new locks. Edit each lock through its context menu.";
      score.RegisterValueChangedCallback(evt =>
      {
        _settings.DefaultRequiredScore = Mathf.Max(0, evt.newValue);
        score.SetValueWithoutNotify(_settings.DefaultRequiredScore);
        RaiseSettingsChanged();
      });
      lockBlock.Add(score);

      var blockerBlock = new Foldout { text = "BLOCKER SETTINGS", value = true };
      blockerBlock.style.marginTop = 8;
      _root.Add(blockerBlock);
      AddPrefabField(blockerBlock, _settings.BlockerVisualPrefab, prefab => _settings.BlockerVisualPrefab = prefab);
      var hits = new IntegerField("Default Required Hits") { value = _settings.DefaultRequiredHits, isDelayed = true };
      hits.tooltip = "Initial number of damage events needed for a new blocker. Set each cell through Required Hits in its menu.";
      hits.RegisterValueChangedCallback(evt =>
      {
        _settings.DefaultRequiredHits = Mathf.Max(1, evt.newValue);
        hits.SetValueWithoutNotify(_settings.DefaultRequiredHits);
        RaiseSettingsChanged();
      });
      blockerBlock.Add(hits);
      blockerBlock.Add(new HelpBox("Add Blocker to the prefab root and assign its Sprite Renderer and Sprite List. Required Hits selects Sprite List[Required Hits - 1]. Damage behavior is implemented later.", HelpBoxMessageType.Info));
      var blockerWarning = new HelpBox("Default Required Hits has no assigned sprite in the Blocker list. Fill the corresponding list entry or change the default.", HelpBoxMessageType.Warning);
      void RefreshBlockerWarning()
      {
        var blocker = _settings.BlockerVisualPrefab != null
          ? _settings.BlockerVisualPrefab.GetComponent<Blocker>() : null;
        blockerWarning.style.display = blocker != null && !blocker.TryGetSprite(_settings.DefaultRequiredHits, out _)
          ? DisplayStyle.Flex : DisplayStyle.None;
      }
      SettingsChanged += RefreshBlockerWarning;
      RefreshBlockerWarning();
      blockerBlock.Add(blockerWarning);
      var legacyBlocker = new Foldout { text = "Legacy Sprite Fallback", value = false };
      AddSpriteField(legacyBlocker, "Sprite", _settings.BlockerSprite, sprite => _settings.BlockerSprite = sprite);
      blockerBlock.Add(legacyBlocker);
      AddCoverFloat(blockerBlock, "Scale", _settings.BlockerScale, 0.0001f,
        value => _settings.BlockerScale = value,
        "Blocker size relative to the cell footprint; preserves sprite aspect ratio.");
      AddCoverFloat(blockerBlock, "Elevation", _settings.BlockerElevation, 0f,
        value => _settings.BlockerElevation = value,
        "Blocker height above the model as a fraction of the smaller cell dimension.");
      _root.Add(new HelpBox("Choose Hex Type from a cell's context menu. Covers are level data only; unlocking and hammer behavior are not implemented.", HelpBoxMessageType.Info));
    }

    private void AddPrefabField(VisualElement parent, GameObject value, Action<GameObject> assign)
    {
      var field = new ObjectField("Visual Prefab") { objectType = typeof(GameObject), allowSceneObjects = false, value = value };
      field.RegisterValueChangedCallback(evt =>
      {
        var prefab = evt.newValue as GameObject;
        if (prefab != null && !PrefabUtility.IsPartOfPrefabAsset(prefab))
        {
          field.SetValueWithoutNotify(evt.previousValue);
          return;
        }
        assign(prefab);
        RaiseSettingsChanged();
      });
      parent.Add(field);
      var canvasWarning = new HelpBox("This prefab contains a Screen Space Canvas. Use World Space for preview and exported grid visuals.", HelpBoxMessageType.Warning);
      void RefreshCanvasWarning()
      {
        bool invalid = false;
        var prefab = field.value as GameObject;
        if (prefab != null)
          foreach (var canvas in prefab.GetComponentsInChildren<Canvas>(true))
            invalid |= canvas.renderMode != RenderMode.WorldSpace;
        canvasWarning.style.display = invalid ? DisplayStyle.Flex : DisplayStyle.None;
      }
      SettingsChanged += RefreshCanvasWarning;
      RefreshCanvasWarning();
      parent.Add(canvasWarning);
    }

    private void AddCoverFloat(VisualElement parent, string label, float value, float minimum,
      Action<float> assign, string tooltip)
    {
      var field = new FloatField(label) { value = value, isDelayed = true, tooltip = tooltip };
      field.RegisterValueChangedCallback(evt =>
      {
        float clamped = Mathf.Max(minimum, evt.newValue);
        assign(clamped);
        field.SetValueWithoutNotify(clamped);
        RaiseSettingsChanged();
      });
      parent.Add(field);
    }

    private void AddSpriteField(VisualElement parent, string label, Sprite value, Action<Sprite> assign)
    {
      var field = new ObjectField(label) { objectType = typeof(Sprite), allowSceneObjects = false, value = value };
      field.RegisterValueChangedCallback(evt =>
      {
        assign(evt.newValue as Sprite);
        RaiseSettingsChanged();
      });
      parent.Add(field);
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
