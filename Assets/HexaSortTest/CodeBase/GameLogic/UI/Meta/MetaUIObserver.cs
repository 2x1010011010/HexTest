using UnityEngine.UI;
using System.Collections.Generic;
using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.Data;
using HexaSortTest.CodeBase.GameLogic.Meta;
using HexaSortTest.CodeBase.Infrastructure.Services.MetaProgressService;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace HexaSortTest.CodeBase.GameLogic.UI.Meta
{
  public class MetaUIObserver : MonoBehaviour
  {
    [SerializeField, BoxGroup("LIST SETUP")] private MetaTileConfigsList _tileConfigsList;
    [SerializeField, BoxGroup("LIST SETUP")] private Transform _listContainer;
    [SerializeField, BoxGroup("LIST SETUP")] private MetaTileListItemButton _listItemPrefab;

    [SerializeField, BoxGroup("LIST SETUP")] private ScrollRect _locationsScroll;

    [SerializeField, BoxGroup("TILE INTERACTION")] private MetaTileInteractor _tileInteractor;

    [Inject] private IMetaObjectFactory _metaObjectFactory;
    [Inject] private IPersistentProgressService _progressService;

    [SerializeField] private MetaLocationViewport _viewport;

    private void RefreshProgress(MetaTile tile)
    {
      if (_selectedItem == null || tile == null) return;
      int total = tile.UsesCells ? tile.Cells.Count : tile.GroupsCount;
      int opened = tile.UsesCells ? tile.Cells.FindAll(tile.IsCellOpen).Count : tile.CurrentGroupIndex;
      _selectedItem.SetProgress(opened, total);
    }

    private readonly List<MetaTileListItemButton> _listItems = new();
    private readonly Dictionary<MetaTileListItemButton, int> _itemIndices = new();
    private MetaTile _activeTile;
    private MetaTileListItemButton _selectedItem;

    private void Start()
    {
      BuildTileList();
      // Continue the first unfinished available location, or the last completed one.
      MetaTileListItemButton selected = null;
      foreach (var item in _listItems)
      {
        if (!_itemIndices.TryGetValue(item, out int index) || !IsTileUnlocked(index)) continue;
        selected = item;
        if (!IsTileUnlocked(index + 1)) break;
      }
      if (selected != null) HandleItemClicked(selected);
    }

    private void OnDestroy()
    {
      ClearTileList();

      if (_activeTile != null)
      {
        _activeTile.OnTileFullyOpened -= HandleTileFullyOpened;
        _activeTile.OnProgressChanged -= RefreshProgress;
      }
    }

    private void BuildTileList()
    {
      ClearTileList();

      if (_tileConfigsList == null || _tileConfigsList.Tiles == null)
      {
        Debug.LogError("[MetaUIObserver] _tileConfigsList is not assigned or has no tiles!");
        return;
      }

      if (_listItemPrefab == null || _listContainer == null)
      {
        Debug.LogError("[MetaUIObserver] _listItemPrefab or _listContainer is not assigned!");
        return;
      }

      for (int i = 0; i < _tileConfigsList.Tiles.Count; i++)
      {
        var config = _tileConfigsList.Tiles[i];
        if (config == null)
          continue;

        var item = Instantiate(_listItemPrefab, _listContainer);
        item.Setup(config);
        item.SetLocked(!IsTileUnlocked(i));
        item.OnItemButtonClick += HandleItemClicked;
        RefreshSavedProgress(item);
        item.gameObject.SetActive(true);

        _listItems.Add(item);
        _itemIndices.Add(item, i);
      }

    }

    private void ClearTileList()
    {
      foreach (var item in _listItems)
      {
        if (item == null)
          continue;

        item.OnItemButtonClick -= HandleItemClicked;
        Destroy(item.gameObject);
      }

      _listItems.Clear();
      _itemIndices.Clear();
    }

    private void HandleItemClicked(MetaTileListItemButton item)
    {
      if (!isActiveAndEnabled || item == _selectedItem) return;
      if (item == null || item.Config == null || item.Config.TilePrefab == null)
      {
        Debug.LogError("[MetaUIObserver] Clicked list item has no valid MetaTileConfig/TilePrefab.");
        return;
      }

      // Check the catalog index, not the compacted list of instantiated buttons.
      if (!_itemIndices.TryGetValue(item, out int index) || !IsTileUnlocked(index)) return;
      if (_tileInteractor == null || _metaObjectFactory == null)
      {
        Debug.LogError("[MetaUIObserver] Missing tile interactor or meta factory.", this);
        return;
      }

      // Finish pending visuals and discard old input before the factory destroys the tile.
      _tileInteractor.SetCurrentTile(null);
      if (_activeTile != null)
      {
        _activeTile.OnTileFullyOpened -= HandleTileFullyOpened;
        _activeTile.OnProgressChanged -= RefreshProgress;
      }
      _activeTile = null;
      if (_selectedItem != null) _selectedItem.SetSelected(false);
      _selectedItem = null;

      var tile = _metaObjectFactory.SpawnTile(item.Config, Vector3.zero);
      if (tile == null)
      {
        Debug.LogError("[MetaUIObserver] Failed to spawn MetaTile.");
        _activeTile = null;
        return;
      }

      tile.InitializeFromSave(FindSavedProgress(tile.TileId));
      tile.OnTileFullyOpened += HandleTileFullyOpened;
      _activeTile = tile;
      tile.OnProgressChanged += RefreshProgress;
      _selectedItem = item;
      item.SetSelected(true);
      ScrollToSelected(item);
      RefreshProgress(tile);
      if (_viewport != null) _viewport.SetTile(tile);

      _tileInteractor.SetCurrentTile(tile);
    }

    private void HandleTileFullyOpened(MetaTile tile)
    {
      RefreshProgress(tile);
      RefreshLockStates();
    }

    private void RefreshSavedProgress(MetaTileListItemButton item)
    {
      var tile = item.Config?.TilePrefab;
      if (tile == null) { item.SetProgress(0, 0); return; }
      var saved = FindSavedProgress(tile.TileId);
      int total = tile.UsesCells ? tile.Cells.Count : tile.GroupsCount;
      int opened = 0;
      if (saved != null)
      {
        if (tile.UsesCells && saved.UsesCellIds)
          opened = tile.Cells.FindAll(cell => cell != null && saved.OpenedCellIds != null &&
            saved.OpenedCellIds.Contains(cell.CellId)).Count;
        else opened = saved.UnlockedGroupsCount;
      }
      item.SetProgress(opened, total);
    }

    private void ScrollToSelected(MetaTileListItemButton item)
    {
      if (_locationsScroll == null || _locationsScroll.content == null ||
          _locationsScroll.viewport == null) return;
      Canvas.ForceUpdateCanvases();
      var content = _locationsScroll.content;
      LayoutRebuilder.ForceRebuildLayoutImmediate(content);
      float overflow = content.rect.width - _locationsScroll.viewport.rect.width;
      _locationsScroll.StopMovement();
      if (overflow <= 0f) { _locationsScroll.horizontalNormalizedPosition = 0f; return; }
      var rect = (RectTransform)item.transform;
      float center = content.InverseTransformPoint(rect.TransformPoint(rect.rect.center)).x - content.rect.xMin;
      _locationsScroll.horizontalNormalizedPosition = Mathf.Clamp01(
        (center - _locationsScroll.viewport.rect.width * 0.5f) / overflow);
    }

    private void RefreshLockStates()
    {
      foreach (var item in _listItems)
        if (item != null && _itemIndices.TryGetValue(item, out int index))
          item.SetLocked(!IsTileUnlocked(index));
    }
    
    private bool IsTileUnlocked(int index)
    {
      if (_tileConfigsList?.Tiles == null || index < 0 || index >= _tileConfigsList.Tiles.Count)
        return false;
      if (_tileConfigsList.Tiles[index]?.TilePrefab == null) return false;
      if (index == 0) return true;

      var prevConfig = _tileConfigsList.Tiles[index - 1];
      // A broken predecessor must not silently unlock the following location.
      if (prevConfig?.TilePrefab == null) return false;
      if (!prevConfig.TilePrefab.ValidateLayout(out _)) return false;

      int totalGroups = prevConfig.TilePrefab.GroupsCount;

      var prevProgress = FindSavedProgress(prevConfig.TilePrefab.TileId);
      if (prevProgress == null) return false;
      if (prevProgress.UsesCellIds)
      {
        var cells = prevConfig.TilePrefab.Cells;
        return cells.Count > 0 && cells.TrueForAll(cell => prevProgress.OpenedCellIds != null &&
          prevProgress.OpenedCellIds.Contains(cell.CellId));
      }
      return prevProgress.UnlockedGroupsCount >= totalGroups;
    }

    private MetaTileProgress FindSavedProgress(string tileId) =>
      _progressService?.PlayerProgress?.MetaProgress?.Find(p => p != null && p.TileId == tileId);
  }
}
