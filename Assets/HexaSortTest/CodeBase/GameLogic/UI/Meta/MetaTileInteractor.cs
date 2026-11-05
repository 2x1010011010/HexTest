using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using HexaSortTest.CodeBase.GameLogic.Meta;
using HexaSortTest.CodeBase.Infrastructure.Services.CurrencyService;
using HexaSortTest.CodeBase.Infrastructure.Services.InputService;
using HexaSortTest.CodeBase.Infrastructure.Services.MetaProgressService;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace HexaSortTest.CodeBase.GameLogic.UI.Meta
{
  public class MetaTileInteractor : MonoBehaviour
  {
    [SerializeField, BoxGroup("FEEDBACK")] private MetaHexCoinsView _hexCoinsView;
    [SerializeField, BoxGroup("FEEDBACK")] private MetaInsufficientFundsPopup _insufficientFundsPopup;
    [SerializeField, BoxGroup("FEEDBACK")] private MetaCoinFlightEffect _coinFlight;
    [SerializeField, BoxGroup("SETUP")] private Camera _camera;
    [Inject] private IInputService _inputService;
    [Inject] private IMetaObserver _metaObserver;
    [Inject] private ICurrencyService _currency;
    private MetaTile _currentTile;
    private List<MetaLocationCell> _cells = new();
    private readonly List<RaycastResult> _uiHits = new();
    private readonly HashSet<MetaLocationCell> _pending = new();
    private readonly List<Tween> _flights = new();
    private GameObject _fallbackUI;

    private void Start()
    {
      if (_hexCoinsView == null || _insufficientFundsPopup == null)
        _fallbackUI = MetaFeedbackUI.Create(ref _hexCoinsView, ref _insufficientFundsPopup);
      _hexCoinsView.Bind(_currency);
    }

    public void SetCurrentTile(MetaTile tile)
    {
      CancelPresentation();
      _currentTile = tile;
      _cells = tile != null ? tile.Cells : new List<MetaLocationCell>();
      if (_insufficientFundsPopup != null) _insufficientFundsPopup.Close();
    }

    private void Update()
    {
      if (_currentTile == null || _inputService == null || _currency == null ||
          (_insufficientFundsPopup != null && _insufficientFundsPopup.IsOpen)) return;
      // MobileInputService.Click assumes GetTouch(0) is available.
      if (Application.isMobilePlatform && Input.touchCount == 0) return;
      if (!_inputService.Click()) return; // One press only: holding never buys another cell.
      Vector2 position = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
      if (HitsUI(position)) return;
      if (_camera == null) _camera = Camera.main;
      if (_camera == null) return;
      Ray ray = _camera.ScreenPointToRay(position);
      MetaLocationCell selected = null;
      float closest = float.PositiveInfinity;
      foreach (var cell in _cells)
        if (cell != null && !_currentTile.IsCellOpen(cell) && cell.TryHit(ray, out float distance) && distance < closest)
        { selected = cell; closest = distance; }
      if (selected != null) Purchase(selected);
    }

    private bool HitsUI(Vector2 position)
    {
      if (EventSystem.current == null) return false;
      _uiHits.Clear();
      EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = position }, _uiHits);
      foreach (var hit in _uiHits)
        if (hit.module is UnityEngine.UI.GraphicRaycaster) return true;
      return false;
    }

    private void Purchase(MetaLocationCell cell)
    {
      MetaCellPurchaseResult result;
      try { result = _metaObserver.TryOpenCell(_currentTile, cell); }
      catch (System.Exception exception)
      {
        _hexCoinsView.ResetView();
        if (_currentTile != null && _currentTile.IsCellOpen(cell)) cell.SetOpened(true);
        Debug.LogException(exception, this);
        return;
      }
      if (result == MetaCellPurchaseResult.InsufficientFunds)
      {
        _insufficientFundsPopup.Show(cell.HexCoinsCost, _currency.HexCoins);
        return;
      }
      if (result != MetaCellPurchaseResult.Success) return;
      _pending.Add(cell);
      StartCoroutine(Present(cell, _hexCoinsView.Revision));
    }

    private IEnumerator Present(MetaLocationCell cell, int debitRevision)
    {
      while (_hexCoinsView != null && !_hexCoinsView.IsComplete(debitRevision)) yield return null;
      if (cell != null && cell.isActiveAndEnabled && _coinFlight != null)
      {
        Tween flight = null;
        try { flight = _coinFlight.Play(_hexCoinsView != null ? _hexCoinsView.CoinSource : transform, cell.transform); }
        catch (System.Exception exception) { Debug.LogException(exception, this); }
        if (flight != null)
        {
          _flights.Add(flight);
          while (flight.IsActive() && !flight.IsComplete() && cell != null && cell.isActiveAndEnabled)
            yield return null;
          flight.Kill();
          _flights.Remove(flight);
        }
      }
      if (cell != null)
      {
        if (cell.isActiveAndEnabled) cell.PlayReveal();
        else cell.SetOpened(true);
      }
      _pending.Remove(cell);
    }

    private void CancelPresentation()
    {
      StopAllCoroutines();
      foreach (var flight in _flights) flight?.Kill();
      _flights.Clear();
      foreach (var cell in _pending) if (cell != null) cell.SetOpened(true);
      _pending.Clear();
      if (_hexCoinsView != null) _hexCoinsView.ResetView();
    }
    private void OnEnable() { if (_fallbackUI != null) _fallbackUI.SetActive(true); }
    private void OnDisable()
    {
      CancelPresentation();
      if (_fallbackUI != null) _fallbackUI.SetActive(false);
    }
    private void OnDestroy() { if (_fallbackUI != null) Destroy(_fallbackUI); }
  }
}
