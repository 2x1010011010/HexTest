using System;
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
    [SerializeField, BoxGroup("LIST SETUP"), Tooltip("The ScrollBox toggled open/closed by the list button.")]
    private GameObject _listPanel;

    [SerializeField, BoxGroup("LIST SETUP")] private MetaTileConfigsList _tileConfigsList;
    [SerializeField, BoxGroup("LIST SETUP")] private Transform _listContainer;
    [SerializeField, BoxGroup("LIST SETUP")] private MetaTileListItemButton _listItemPrefab;

    [SerializeField, BoxGroup("BUTTONS")] private MetaSwitchButton _switchButton;
    [SerializeField, BoxGroup("BUTTONS")] private MetaExitButton _exitButton;

    [SerializeField, BoxGroup("TILE INTERACTION")] private MetaTileInteractor _tileInteractor;

    [Inject] private IMetaObjectFactory _metaObjectFactory;
    [Inject] private IPersistentProgressService _progressService;

    public event Action OnExitRequested;

    private readonly List<MetaTileListItemButton> _listItems = new();
    private readonly Dictionary<MetaTileListItemButton, int> _itemIndices = new();
    private MetaTile _activeTile;
    private bool _isListOpen;

    private void Awake()
    {
      if (_listPanel != null)
        _listPanel.SetActive(false);
    }

    private void Start() =>
      BuildTileList();

    private void OnEnable()
    {
      if (_exitButton != null)
        _exitButton.OnExitButtonClick += HandleExitClicked;
      else
        Debug.LogError("[MetaUIObserver] _exitButton is not assigned in the inspector!");

      if (_switchButton != null)
        _switchButton.OnSwitchButtonClick += HandleSwitchClicked;
      else
        Debug.LogError("[MetaUIObserver] _switchButton is not assigned in the inspector!");
    }

    private void OnDisable()
    {
      if (_exitButton != null)
        _exitButton.OnExitButtonClick -= HandleExitClicked;

      if (_switchButton != null)
        _switchButton.OnSwitchButtonClick -= HandleSwitchClicked;
    }

    private void OnDestroy()
    {
      ClearTileList();

      if (_activeTile != null)
        _activeTile.OnTileFullyOpened -= HandleTileFullyOpened;
    }

    private void HandleSwitchClicked()
    {
      if (_isListOpen)
        HideList();
      else
        ShowList();
    }

    private void ShowList()
    {
      _isListOpen = true;

      if (_listPanel != null)
        _listPanel.SetActive(true);
      else
        Debug.LogError("[MetaUIObserver] _listPanel is not assigned in the inspector! The ScrollBox cannot be shown.");
    }

    private void HideList()
    {
      _isListOpen = false;

      if (_listPanel != null)
        _listPanel.SetActive(false);
    }

    private void HandleExitClicked() =>
      OnExitRequested?.Invoke();

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
        _activeTile.OnTileFullyOpened -= HandleTileFullyOpened;
      _activeTile = null;

      var tile = _metaObjectFactory.SpawnTile(item.Config, Vector3.zero);
      if (tile == null)
      {
        Debug.LogError("[MetaUIObserver] Failed to spawn MetaTile.");
        _activeTile = null;
        return;
      }

      HideList();
      tile.InitializeFromSave(FindSavedProgress(tile.TileId));
      tile.OnTileFullyOpened += HandleTileFullyOpened;
      _activeTile = tile;

      _tileInteractor.SetCurrentTile(tile);
    }

    private void HandleTileFullyOpened(MetaTile tile) =>
      RefreshLockStates();

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
