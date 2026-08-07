using System;
using System.Collections.Generic;
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
    [SerializeField, BoxGroup("SETUP")] private List<MetaTile> _metaTiles = new();
    [SerializeField, BoxGroup("SETUP")] private LayerMask _metaTileLayer;
    [SerializeField, BoxGroup("BUTTONS")] private MetaExitButton _exitButton;

    [Inject] private IMetaObserver _metaObserver;
    [Inject] private IPersistentProgressService _progressService;

    public event Action OnExitRequested;

    private Camera _camera;

    private void Awake() =>
      _camera = Camera.main;

    private void Start() =>
      InitializeTiles();

    private void OnEnable()
    {
      if (_exitButton != null)
        _exitButton.OnExitButtonClick += HandleExitClicked;
    }

    private void OnDisable()
    {
      if (_exitButton != null)
        _exitButton.OnExitButtonClick -= HandleExitClicked;

      foreach (var tile in _metaTiles)
        if (tile != null)
          tile.OnTileFullyOpened -= HandleTileFullyOpened;
    }

    private void Update()
    {
      if (Input.GetMouseButtonDown(0))
        TryPickTile(Input.mousePosition);
    }

    private void InitializeTiles()
    {
      if (_progressService.PlayerProgress.MetaProgress == null)
        _progressService.PlayerProgress.MetaProgress = new();

      var savedTiles = _progressService.PlayerProgress.MetaProgress;

      foreach (var tile in _metaTiles)
      {
        if (tile == null)
          continue;

        var saved = savedTiles.Find(p => p.TileId == tile.TileId);
        tile.InitializeFromSave(saved);
        tile.OnTileFullyOpened += HandleTileFullyOpened;
      }
    }

    private void TryPickTile(Vector3 screenPosition)
    {
      if (_camera == null)
        _camera = Camera.main;

      if (_camera == null)
        return;

      var ray = _camera.ScreenPointToRay(screenPosition);
      if (!Physics.Raycast(ray, out var hit, 200f, _metaTileLayer))
        return;

      var tile = hit.collider.GetComponentInParent<MetaTile>();
      if (tile != null)
        ChooseTile(tile);
    }

    public void ChooseTile(MetaTile metaTile)
    {
      if (metaTile == null || metaTile.IsTileOpen)
        return;

      _metaObserver.OpenTile(metaTile);
    }

    public void Exit() =>
      OnExitRequested?.Invoke();

    private void HandleExitClicked() =>
      Exit();

    private void HandleTileFullyOpened(MetaTile tile) =>
      Debug.Log($"[MetaUIObserver] Tile fully opened: {tile.TileId}");
  }
}
