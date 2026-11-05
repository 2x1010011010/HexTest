using System;
using TMPro;
using UnityEngine.UI;
using HexaSortTest.CodeBase.Infrastructure.Services.LivesService;
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
    public event Action<string> OnPlayRequested;
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _closeListButton;
    [SerializeField] private TMP_Text _locationTitle;
    [SerializeField] private TMP_Text _progressText;
    [SerializeField] private Image _progressFill;
    [SerializeField] private MetaLocationViewport _viewport;
    [SerializeField] private GameObject _noLivesPanel;
    [SerializeField] private Button _closeNoLivesButton;
    [Inject] private LivesService _lives;

    private void HandlePlay()
    {
      _lives.Refresh();
      if (_lives.Lives <= 0) { if (_noLivesPanel != null) _noLivesPanel.SetActive(true); return; }
      OnPlayRequested?.Invoke(_progressService.PlayerProgress.WorldData.LastLevel.Level);
    }
    private void CloseNoLives() { if (_noLivesPanel != null) _noLivesPanel.SetActive(false); }
    private void RefreshProgress(MetaTile tile)
    {
      int total = tile.UsesCells ? tile.Cells.Count : tile.GroupsCount;
      int opened = tile.UsesCells ? tile.Cells.FindAll(tile.IsCellOpen).Count : tile.CurrentGroupIndex;
      if (_progressText != null) _progressText.text = $"{opened}/{total}";
      if (_progressFill != null) _progressFill.fillAmount = total > 0 ? (float)opened / total : 0;
    }

    private readonly List<MetaTileListItemButton> _listItems = new();
    private readonly Dictionary<MetaTileListItemButton, int> _itemIndices = new();
    private MetaTile _activeTile;
    private bool _isListOpen;

    private void Awake()
    {
      if (_listPanel != null)
        _listPanel.SetActive(false);
    }

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

    private void OnEnable()
    {
      if (_playButton != null) _playButton.onClick.AddListener(HandlePlay);
      if (_closeListButton != null) _closeListButton.onClick.AddListener(HideList);
      if (_closeNoLivesButton != null) _closeNoLivesButton.onClick.AddListener(CloseNoLives);
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
      if (_playButton != null) _playButton.onClick.RemoveListener(HandlePlay);
      if (_closeListButton != null) _closeListButton.onClick.RemoveListener(HideList);
      if (_closeNoLivesButton != null) _closeNoLivesButton.onClick.RemoveListener(CloseNoLives);
      if (_exitButton != null)
        _exitButton.OnExitButtonClick -= HandleExitClicked;

      if (_switchButton != null)
        _switchButton.OnSwitchButtonClick -= HandleSwitchClicked;
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
        var rect = item.transform as RectTransform;
        if (rect != null)
        {
          rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(1, 1);
          rect.pivot = new Vector2(0.5f, 1);
          rect.sizeDelta = new Vector2(-16, 260);
          rect.anchoredPosition = new Vector2(0, -_listItems.Count * 280);
        }
        item.Setup(config);
        item.SetLocked(!IsTileUnlocked(i));
        item.OnItemButtonClick += HandleItemClicked;

        _listItems.Add(item);
        _itemIndices.Add(item, i);
      }
      if (_listContainer is RectTransform container)
        container.sizeDelta = new Vector2(container.sizeDelta.x, Mathf.Max(0, _listItems.Count * 280));
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
      {
        _activeTile.OnTileFullyOpened -= HandleTileFullyOpened;
        _activeTile.OnProgressChanged -= RefreshProgress;
      }
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
      tile.OnProgressChanged += RefreshProgress;
      if (_locationTitle != null) _locationTitle.text = item.Config.DisplayName;
      RefreshProgress(tile);
      if (_viewport != null) _viewport.SetTile(tile);

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
