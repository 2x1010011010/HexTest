using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HexaSortTest.CodeBase.GameLogic.Meta
{
  [DisallowMultipleComponent, RequireComponent(typeof(MetaTile))]
  public class MetaLocationAuthoring : MonoBehaviour
  {
    public enum GridPlane { XY, XZ }
    public enum HexOrientation { PointyTop, FlatTop }

    [SerializeField, BoxGroup("LOCATION"), Tooltip("Unique and permanent save key. Use a new ID for a redesigned location.")]
    private string _locationId;
    [SerializeField, BoxGroup("LOCATION"), Min(0), LabelText("Default HexCoins Cost"), Tooltip("Used only for new cells. Existing cell prices are preserved.")] private int _hexCoinsPerCover = 10;
    [SerializeField, BoxGroup("PICTURE")] private SpriteRenderer _pictureRenderer;
    [SerializeField, BoxGroup("PICTURE")] private Sprite _picture;
    [SerializeField, BoxGroup("SPRITES"), LabelText("MaskSprite")] private Sprite _maskSprite;
    [SerializeField, BoxGroup("SPRITES"), LabelText("CoverSprite")] private Sprite _coverSprite;
    [SerializeField, BoxGroup("SPRITES"), LabelText("Lock")] private Sprite _lockSprite;
    [SerializeField, BoxGroup("APPEARANCE"), Range(0.5f, 1f), Tooltip("Filled mask size relative to the frame. Match the inner opening of the golden frame.")]
    private float _maskInset = 0.94f;
    [SerializeField, BoxGroup("APPEARANCE"), Range(0.1f, 1f)] private float _lockSize = 0.3f;
    [SerializeField, BoxGroup("APPEARANCE")] private Vector2 _lockOffset = new Vector2(0f, 0.12f);
    [SerializeField, BoxGroup("APPEARANCE")] private Vector2 _priceOffset = new Vector2(0f, -0.23f);
    [SerializeField, BoxGroup("APPEARANCE")] private TMP_FontAsset _priceFont;
    [SerializeField, BoxGroup("APPEARANCE"), Min(0.1f)] private float _priceFontSize = 4f;
    [SerializeField, BoxGroup("APPEARANCE")] private Color _priceColor = Color.white;
    [SerializeField, BoxGroup("GRID"), Min(1)] private int _columns = 5;
    [SerializeField, BoxGroup("GRID"), Min(1)] private int _rows = 5;
    [SerializeField, BoxGroup("GRID")] private GridPlane _plane = GridPlane.XY;
    [SerializeField, BoxGroup("GRID")] private HexOrientation _orientation = HexOrientation.PointyTop;
    [SerializeField, BoxGroup("GRID"), Tooltip("Full width and height of one cover after scaling, in local units.")]
    private Vector2 _cellSize = new Vector2(1f, 1.1547f);
    [SerializeField, BoxGroup("GRID"), Tooltip("1 = touching hexes; slightly below 1 hides tiny seams."), Range(0.5f, 1.5f)]
    private float _spacing = 1f;
    [SerializeField, BoxGroup("GRID")] private Vector3 _offset;
    [SerializeField, BoxGroup("GRID")] private Vector3 _coverEuler;
    [SerializeField, BoxGroup("GRID")] private Vector3 _coverScale = Vector3.one;
    [SerializeField, BoxGroup("GENERATED"), ReadOnly] private Transform _generatedRoot;
    [SerializeField, BoxGroup("PREVIEW"), Min(0)] private int _previewOpenedCount;

#if UNITY_EDITOR
    [ShowInInspector, ReadOnly, BoxGroup("GENERATED")]
    private int CoverCount => _generatedRoot == null ? 0 : _generatedRoot.childCount;
    [ShowInInspector, ReadOnly, BoxGroup("GENERATED")]
    private long TotalHexCoins
    {
      get
      {
        long total = 0;
        if (_generatedRoot != null)
          foreach (Transform child in _generatedRoot)
            if (child.TryGetComponent<MetaLocationCell>(out var cell)) total += cell.HexCoinsCost;
        return total;
      }
    }

    private bool CanEdit()
    {
      if (Application.isPlaying || EditorUtility.IsPersistent(gameObject))
      {
        Debug.LogWarning("Open the prefab in Prefab Mode or select a scene instance outside Play Mode.", this);
        return false;
      }
      if (_generatedRoot != null && (_generatedRoot.parent != transform || _generatedRoot == transform))
      {
        Debug.LogError("Generated root must be a direct child of this location.", this);
        return false;
      }
      return true;
    }

    private void Reset()
    {
      _locationId = GetComponent<MetaTile>().TileId;
      if (string.IsNullOrWhiteSpace(_locationId)) _locationId = System.Guid.NewGuid().ToString("N");
      _hexCoinsPerCover = GetComponent<MetaTile>().HexCoinsCostPerStep;
    }

    [Button("Generate Grid"), BoxGroup("ACTIONS"), DisableInPlayMode]
    public void GenerateGrid()
    {
      if (!CanEdit()) return;
      if (_generatedRoot != null)
      {
        Debug.LogWarning("Grid already exists. Edit it manually or use Clear Generated Covers before regenerating.", this);
        return;
      }
      if (!HasSpriteSettings() ||
          _columns < 1 || _rows < 1 || (long)_columns * _rows > 10000 ||
          _cellSize.x <= 0f || _cellSize.y <= 0f || _spacing <= 0f ||
          _coverScale.x <= 0f || _coverScale.y <= 0f || _coverScale.z <= 0f)
      {
        Debug.LogError("Assign MaskSprite, CoverSprite, Lock and a TMP font (or TMP default font). Check grid dimensions (max 10000), cell size and scale.", this);
        return;
      }
      if (!HasValidPicture()) return;
      int undo = BeginUndo("Generate meta covers");
      Undo.RecordObject(this, "Generated root");
      var root = new GameObject("Generated Covers");
      Undo.RegisterCreatedObjectUndo(root, "Create covers root");
      root.transform.SetParent(transform, false);
      _generatedRoot = root.transform;
      Vector2 min = Vector2.positiveInfinity, max = Vector2.negativeInfinity;
      for (int row = 0; row < _rows; row++)
        for (int col = 0; col < _columns; col++)
        {
          Vector2 point = GridPoint(col, row);
          min = Vector2.Min(min, point);
          max = Vector2.Max(max, point);
        }
      Vector2 center = (min + max) * 0.5f;
      for (int row = 0; row < _rows; row++)
        for (int col = 0; col < _columns; col++)
        {
          var cellObject = CreateChild($"Cell_{row:D2}_{col:D2}", _generatedRoot);
          Vector2 point = GridPoint(col, row) - center;
          cellObject.transform.localPosition = _offset + (_plane == GridPlane.XY
            ? new Vector3(point.x, point.y, 0f) : new Vector3(point.x, 0f, point.y));
          cellObject.transform.localRotation = Quaternion.Euler(_coverEuler);
          cellObject.transform.localScale = _coverScale;
          BuildCell(cellObject);
        }
      ApplySettingsInternal();
      FinishUndo(undo);
    }

    private Vector2 GridPoint(int col, int row) => _orientation == HexOrientation.PointyTop
      ? new Vector2((col + (row % 2) * 0.5f) * _cellSize.x, row * _cellSize.y * 0.75f) * _spacing
      : new Vector2(col * _cellSize.x * 0.75f, (row + (col % 2) * 0.5f) * _cellSize.y) * _spacing;

    [Button("Collect Covers / Apply Settings"), BoxGroup("ACTIONS"), DisableInPlayMode]
    public void ApplySettings()
    {
      if (!CanEdit()) return;
      if (_generatedRoot == null) { Debug.LogWarning("Generate the grid first.", this); return; }
      if (!HasSpriteSettings())
      { Debug.LogError("Assign MaskSprite, CoverSprite, Lock and a TMP font (or TMP default font).", this); return; }
      if (!HasValidPicture()) return;
      if (!TryCollectCovers(out _)) return;
      int undo = BeginUndo("Apply meta settings");
      ApplySettingsInternal();
      FinishUndo(undo);
    }

    private void ApplySettingsInternal()
    {
      var tile = GetComponent<MetaTile>();
      Undo.RecordObject(this, "Meta authoring settings");
      if (string.IsNullOrWhiteSpace(_locationId))
        _locationId = string.IsNullOrWhiteSpace(tile.TileId) ? System.Guid.NewGuid().ToString("N") : tile.TileId;
      if (!TryCollectCovers(out var covers)) return;
      if (_generatedRoot != null)
        foreach (Transform child in _generatedRoot) UpdateCellVisuals(child.GetComponent<MetaLocationCell>());
      ConfigurePictureMask();
      RecordCoverVisibility();
      Undo.RecordObject(tile, "Configure meta tile");
      tile.ConfigureAuthoring(_locationId, _hexCoinsPerCover, covers);
      _previewOpenedCount = 0;
      MarkChanged(tile);
      MarkCoverVisibility();
      if (_pictureRenderer != null && _picture != null)
      {
        Undo.RecordObject(_pictureRenderer, "Set location picture");
        _pictureRenderer.sprite = _picture;
        MarkChanged(_pictureRenderer);
      }
      MarkChanged(this);
    }

    [Button("Validate Location"), BoxGroup("ACTIONS"), DisableInPlayMode]
    public void ValidateLocation()
    {
      var tile = GetComponent<MetaTile>();
      if (!tile.ValidateLayout(out string error)) { Debug.LogError(error, this); return; }
      if (_generatedRoot == null || _generatedRoot.parent != transform)
      { Debug.LogError("Missing generated root or invalid parent.", this); return; }
      if (!TryCollectCovers(out var covers)) return;
      if (!tile.MatchesAuthoringCovers(covers))
      { Debug.LogError("Cover order or count changed. Click Collect Covers / Apply Settings.", this); return; }
      if (_pictureRenderer == null || !_pictureRenderer.transform.IsChildOf(transform) ||
          _pictureRenderer.transform.IsChildOf(_generatedRoot))
      { Debug.LogError("Assign a picture renderer inside the location, outside Generated Covers.", this); return; }
      if (_pictureRenderer.sprite == null) { Debug.LogError("Picture renderer has no sprite.", this); return; }
      if (GetComponentInChildren<Collider>(true) == null)
      { Debug.LogError("Add a 3D Collider on the location for MetaTileInteractor and set its layer.", this); return; }
      if (tile.TileId != _locationId)
      { Debug.LogError("Settings changed. Click Collect Covers / Apply Settings.", this); return; }
      if (!HasSpriteSettings() || _pictureRenderer.maskInteraction != SpriteMaskInteraction.VisibleInsideMask ||
          GetComponent<SortingGroup>() == null)
      { Debug.LogError("Assign all sprites/font and click Collect Covers / Apply Settings.", this); return; }
      Debug.Log($"Meta location valid: {CoverCount} covers, {TotalHexCoins} HexCoins. Also check unique Location Id in the location list and the input layer.", this);
    }

    [Button("Preview Opened Count"), BoxGroup("PREVIEW"), DisableInPlayMode]
    public void Preview()
    {
      if (!CanEdit()) return;
      int undo = BeginUndo("Preview meta");
      RecordCoverVisibility();
      GetComponent<MetaTile>().PreviewOpenedCount(_previewOpenedCount);
      MarkCoverVisibility();
      FinishUndo(undo);
    }

    [Button("Show All Covers"), BoxGroup("PREVIEW"), DisableInPlayMode]
    public void ShowAllCovers()
    {
      if (!CanEdit()) return;
      Undo.RecordObject(this, "Reset preview");
      _previewOpenedCount = 0;
      Preview();
      MarkChanged(this);
    }

    [Button("Clear Generated Covers"), BoxGroup("ACTIONS"), DisableInPlayMode]
    public void ClearGeneratedCovers()
    {
      if (!CanEdit() || _generatedRoot == null) return;
      int undo = BeginUndo("Clear meta covers");
      Undo.RecordObject(this, "Clear generated root");
      Undo.DestroyObjectImmediate(_generatedRoot.gameObject);
      _generatedRoot = null;
      ApplySettingsInternal();
      FinishUndo(undo);
    }

    [Button("Save Location As Prefab"), BoxGroup("ACTIONS"), DisableInPlayMode]
    public void SaveAsPrefab()
    {
      if (!CanEdit()) return;
      if (!GetComponent<MetaTile>().ValidateLayout(out string error))
      { Debug.LogError(error, this); return; }
      string path = EditorUtility.SaveFilePanelInProject("Save meta location", gameObject.name, "prefab", "Choose a new prefab path.");
      if (string.IsNullOrEmpty(path)) return;
      // Never overwrite an unrelated prefab (or lose its GUID) through this convenience action.
      if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
      { Debug.LogWarning("For an existing prefab use Prefab Mode Save or Apply. Choose a new path here.", this); return; }
      ShowAllCovers();
      PrefabUtility.SaveAsPrefabAsset(gameObject, path);
    }

    private void RecordCoverVisibility()
    {
      if (_generatedRoot == null) return;
      foreach (Transform child in _generatedRoot)
        if (child.TryGetComponent<MetaLocationCell>(out var cell) && cell.CoverRoot != null)
          Undo.RecordObject(cell.CoverRoot, "Cover visibility");
    }

    private void MarkCoverVisibility()
    {
      if (_generatedRoot == null) return;
      foreach (Transform child in _generatedRoot)
        if (child.TryGetComponent<MetaLocationCell>(out var cell) && cell.CoverRoot != null)
          MarkChanged(cell.CoverRoot);
    }

    private bool HasSpriteSettings() => _maskSprite != null && _coverSprite != null && _lockSprite != null &&
      (_priceFont != null || TMP_Settings.defaultFontAsset != null);

    private bool HasValidPicture()
    {
      if (_pictureRenderer != null && _pictureRenderer.transform.IsChildOf(transform) &&
          (_generatedRoot == null || !_pictureRenderer.transform.IsChildOf(_generatedRoot)) &&
          (_picture != null || _pictureRenderer.sprite != null)) return true;
      Debug.LogError("Assign Picture Renderer inside the location, outside Generated Covers, and its picture sprite.", this);
      return false;
    }

    private bool TryCollectCovers(out List<GameObject> covers)
    {
      covers = new List<GameObject>();
      if (_generatedRoot == null) return true;
      foreach (Transform child in _generatedRoot)
      {
        if (!child.TryGetComponent<MetaLocationCell>(out var cell) || !cell.HasVisuals ||
            cell.CoverRoot.transform.parent != child)
        {
          Debug.LogError("Invalid or old cell layout. Regenerate an old prefab-based grid, or restore missing cell references.", child);
          return false;
        }
        covers.Add(cell.CoverRoot);
      }
      return true;
    }

    private GameObject CreateChild(string label, Transform parent)
    {
      var go = new GameObject(label);
      Undo.RegisterCreatedObjectUndo(go, "Create meta cell object");
      go.transform.SetParent(parent, false);
      return go;
    }

    private void BuildCell(GameObject go)
    {
      var cell = Undo.AddComponent<MetaLocationCell>(go);
      var mask = Undo.AddComponent<SpriteMask>(CreateChild("Picture Mask", go.transform));
      var frame = Undo.AddComponent<SpriteRenderer>(CreateChild("Golden Frame", go.transform));
      var coverRoot = CreateChild("Cover", go.transform);
      var cover = Undo.AddComponent<SpriteRenderer>(CreateChild("Hexagon", coverRoot.transform));
      var lockRenderer = Undo.AddComponent<SpriteRenderer>(CreateChild("Lock", coverRoot.transform));
      var price = Undo.AddComponent<TextMeshPro>(CreateChild("Price", coverRoot.transform));
      Undo.RecordObject(cell, "Configure cell");
      cell.Configure(_hexCoinsPerCover, coverRoot, frame, mask, cover, lockRenderer, price);
      MarkChanged(cell);
    }

    private void ConfigurePictureMask()
    {
      if (_pictureRenderer == null) return;
      if (GetComponent<SortingGroup>() == null) Undo.AddComponent<SortingGroup>(gameObject);
      Undo.RecordObject(_pictureRenderer, "Configure picture masking");
      _pictureRenderer.maskInteraction = _generatedRoot != null ? SpriteMaskInteraction.VisibleInsideMask : SpriteMaskInteraction.None;
      _pictureRenderer.sortingLayerID = 0;
      _pictureRenderer.sortingOrder = 0;
      MarkChanged(_pictureRenderer);
    }

    private void UpdateCellVisuals(MetaLocationCell cell)
    {
      SetSprite(cell.Frame, _maskSprite, _cellSize, 1);
      SetSprite(cell.Cover, _coverSprite, _cellSize, 2);
      float lockWidth = Mathf.Min(_cellSize.x, _cellSize.y) * _lockSize;
      float aspect = _lockSprite.bounds.size.y / Mathf.Max(0.0001f, _lockSprite.bounds.size.x);
      SetSprite(cell.Lock, _lockSprite, new Vector2(lockWidth, lockWidth * aspect), 3);
      cell.Lock.transform.localPosition += new Vector3(_lockOffset.x * _cellSize.x, _lockOffset.y * _cellSize.y, -0.01f);
      MarkChanged(cell.Lock.transform);
      Undo.RecordObject(cell.Mask, "Configure filled mask");
      // A hollow golden frame would mask OUT its transparent centre. Use the solid hex silhouette.
      cell.Mask.sprite = _coverSprite;
      cell.Mask.alphaCutoff = 0.5f;
      cell.Mask.isCustomRangeActive = true;
      cell.Mask.backSortingLayerID = 0;
      cell.Mask.frontSortingLayerID = 0;
      cell.Mask.backSortingOrder = -1;
      cell.Mask.frontSortingOrder = 1;
      FitSpriteTransform(cell.Mask.transform, _coverSprite, _cellSize * _maskInset);
      MarkChanged(cell.Mask);
      var label = cell.PriceLabel;
      Undo.RecordObject(label, "Configure price label");
      Undo.RecordObject(label.rectTransform, "Position price label");
      Undo.RecordObject(label.GetComponent<MeshRenderer>(), "Sort price label");
      label.font = _priceFont != null ? _priceFont : TMP_Settings.defaultFontAsset;
      label.color = _priceColor;
      label.fontSize = _priceFontSize;
      label.enableAutoSizing = true;
      label.fontSizeMin = 0.1f;
      label.fontSizeMax = _priceFontSize;
      label.alignment = TextAlignmentOptions.Center;
      label.overflowMode = TextOverflowModes.Overflow;
      label.rectTransform.sizeDelta = new Vector2(_cellSize.x * 0.8f, _cellSize.y * 0.25f);
      label.transform.localPosition = new Vector3(_priceOffset.x * _cellSize.x, _priceOffset.y * _cellSize.y, -0.02f);
      label.GetComponent<MeshRenderer>().sortingLayerID = 0;
      label.GetComponent<MeshRenderer>().sortingOrder = 4;
      cell.RefreshPrice();
      MarkChanged(label);
      MarkChanged(label.rectTransform);
      MarkChanged(label.GetComponent<MeshRenderer>());
    }

    private static void SetSprite(SpriteRenderer renderer, Sprite sprite, Vector2 size, int order)
    {
      Undo.RecordObject(renderer, "Update cell sprite");
      renderer.sprite = sprite;
      renderer.maskInteraction = SpriteMaskInteraction.None;
      renderer.sortingLayerID = 0;
      renderer.sortingOrder = order;
      FitSpriteTransform(renderer.transform, sprite, size);
      MarkChanged(renderer);
    }

    private static void FitSpriteTransform(Transform target, Sprite sprite, Vector2 size)
    {
      Undo.RecordObject(target, "Fit sprite");
      var bounds = sprite.bounds;
      var scale = new Vector3(size.x / Mathf.Max(0.0001f, bounds.size.x),
        size.y / Mathf.Max(0.0001f, bounds.size.y), 1f);
      target.localScale = scale;
      target.localPosition = -Vector3.Scale(bounds.center, scale);
      MarkChanged(target);
    }

    private static int BeginUndo(string label)
    {
      Undo.IncrementCurrentGroup();
      Undo.SetCurrentGroupName(label);
      return Undo.GetCurrentGroup();
    }
    private void FinishUndo(int group)
    {
      MarkChanged(this);
      Undo.CollapseUndoOperations(group);
    }
    private static void MarkChanged(Object target)
    {
      EditorUtility.SetDirty(target);
      PrefabUtility.RecordPrefabInstancePropertyModifications(target);
    }
#endif
  }
}
