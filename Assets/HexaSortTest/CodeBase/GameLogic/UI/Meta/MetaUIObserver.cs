using System;
using System.Collections.Generic;
using HexaSortTest.CodeBase.GameConfigs;
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
    [SerializeField, BoxGroup("TILE CONFIGS")] private MetaTileConfigsList _tileConfigs;

    [SerializeField, BoxGroup("SPAWN SETUP")] private Vector3 _spawnPoint = Vector3.zero;
    [SerializeField, BoxGroup("SPAWN SETUP")] private LayerMask _metaTileLayer;

    [SerializeField, BoxGroup("LIST SETUP")] private GameObject _listPanel;
    [SerializeField, BoxGroup("LIST SETUP")] private Transform _listContainer;
    [SerializeField, BoxGroup("LIST SETUP")] private MetaTileListItemButton _listItemPrefab;

    [SerializeField, BoxGroup("BUTTONS")] private MetaSwitchButton _switchButton;
    [SerializeField, BoxGroup("BUTTONS")] private MetaExitButton _exitButton;

    [Inject] private IMetaObjectFactory _metaObjectFactory;
    [Inject] private IMetaObserver _metaObserver;
    [Inject] private IPersistentProgressService _progressService;

    public event Action OnExitRequested;

    private readonly List<MetaTileListItemButton> _listItems = new();

    private Camera _camera;
    private MetaTileConfig _currentConfig;
    private MetaTile _currentTile;
    private bool _isListOpen;

    private void Awake() =>
      _camera = Camera.main;

    private void Start()
    {
      if (_switchButton != null)
        _switchButton.gameObject.SetActive(false);

      BuildList();
      ShowList();
    }

    private void OnEnable()
    {
      if (_exitButton != null)
        _exitButton.OnExitButtonClick += HandleExitClicked;

      if (_switchButton != null)
        _switchButton.OnSwitchButtonClick += HandleSwitchClicked;
    }

    private void OnDisable()
    {
      if (_exitButton != null)
        _exitButton.OnExitButtonClick -= HandleExitClicked;

      if (_switchButton != null)
        _switchButton.OnSwitchButtonClick -= HandleSwitchClicked;

      foreach (var item in _listItems)
        if (item != null)
          item.OnItemButtonClick -= HandleListItemClicked;

      if (_currentTile != null)
        _currentTile.OnTileFullyOpened -= HandleCurrentTileFullyOpened;
    }

    private void Update()
    {
      // Don't let taps fall through to the 3D tile while the list is open.
      if (_isListOpen)
        return;

      if (Input.GetMouseButtonDown(0))
        TryTapCurrentTile(Input.mousePosition);
    }

    private void BuildList()
    {
      if (_tileConfigs == null || _tileConfigs.Tiles == null)
      {
        Debug.LogError("[MetaUIObserver] _tileConfigs is not assigned!");
        return;
      }

      if (_listItemPrefab == null || _listContainer == null)
      {
        Debug.LogError("[MetaUIObserver] _listItemPrefab or _listContainer is not assigned!");
        return;
      }

      foreach (var config in _tileConfigs.Tiles)
      {
        if (config == null || config.TilePrefab == null)
          continue;

        var item = Instantiate(_listItemPrefab, _listContainer);
        item.Setup(config);
        item.OnItemButtonClick += HandleListItemClicked;
        _listItems.Add(item);
      }
    }

    private void HandleListItemClicked(MetaTileListItemButton item)
    {
      if (item.Config == _currentConfig)
      {
        // Re-picking the currently spawned tile just closes the list again.
        HideList();
        return;
      }

      // Defensive guard: items for other tiles should already be disabled
      // via SetLocked while the current tile isn't fully open, but don't
      // rely solely on UI state.
      if (_currentTile != null && !_currentTile.IsTileOpen)
        return;

      SelectTile(item.Config);
    }

    private void SelectTile(MetaTileConfig config)
    {
      if (_currentTile != null)
        _currentTile.OnTileFullyOpened -= HandleCurrentTileFullyOpened;

      _currentTile = _metaObjectFactory.SpawnTile(config, _spawnPoint);
      _currentConfig = config;

      if (_currentTile == null)
      {
        Debug.LogError("[MetaUIObserver] Failed to spawn tile from selected config.");
        return;
      }

      var saved = _progressService.PlayerProgress.MetaProgress?.Find(p => p.TileId == _currentTile.TileId);
      _currentTile.InitializeFromSave(saved);
      _currentTile.OnTileFullyOpened += HandleCurrentTileFullyOpened;

      if (_switchButton != null)
        _switchButton.gameObject.SetActive(true);

      HideList();
    }

    private void HandleCurrentTileFullyOpened(MetaTile tile) =>
      Debug.Log($"[MetaUIObserver] Tile fully opened: {tile.TileId}");

    private void HandleSwitchClicked()
    {
      if (_isListOpen)
      {
        HideList();
        return;
      }

      RefreshLockStates();
      ShowList();
    }

    private void ShowList()
    {
      _isListOpen = true;

      if (_listPanel != null)
        _listPanel.SetActive(true);
    }

    private void HideList()
    {
      _isListOpen = false;

      if (_listPanel != null)
        _listPanel.SetActive(false);
    }

    private void RefreshLockStates()
    {
      bool locked = _currentTile != null && !_currentTile.IsTileOpen;

      foreach (var item in _listItems)
        item.SetLocked(locked && item.Config != _currentConfig);
    }

    private void TryTapCurrentTile(Vector3 screenPosition)
    {
      if (_currentTile == null)
        return;

      if (_camera == null)
        _camera = Camera.main;

      if (_camera == null)
        return;

      var ray = _camera.ScreenPointToRay(screenPosition);
      if (!Physics.Raycast(ray, out var hit, 200f, _metaTileLayer))
        return;

      var tile = hit.collider.GetComponentInParent<MetaTile>();
      if (tile != null && tile == _currentTile)
        _metaObserver.OpenTile(tile);
    }

    public void Exit() =>
      OnExitRequested?.Invoke();

    private void HandleExitClicked() =>
      Exit();
  }
}
