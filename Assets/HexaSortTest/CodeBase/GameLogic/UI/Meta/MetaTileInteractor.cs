using HexaSortTest.CodeBase.GameLogic.Meta;
using HexaSortTest.CodeBase.Infrastructure.Services.InputService;
using HexaSortTest.CodeBase.Infrastructure.Services.MetaProgressService;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace HexaSortTest.CodeBase.GameLogic.UI.Meta
{
  public class MetaTileInteractor : MonoBehaviour
  {
    [SerializeField, BoxGroup("SETUP"), Tooltip("Physics layer the MetaTile's collider(s) live on.")]
    private LayerMask _tileLayer;

    [SerializeField, BoxGroup("HOLD SETTINGS"),
     Tooltip("Seconds between spends while held down. Lower = faster/'accelerated' spending vs a single tap.")]
    private float _holdStepInterval = 0.15f;

    [Inject] private IInputService _inputService;
    [Inject] private IMetaObserver _metaObserver;

    private Camera _camera;
    private MetaTile _currentTile;
    private bool _isPressingTile;
    private float _holdTimer;

    private void Awake() =>
      _camera = Camera.main;

    public void SetCurrentTile(MetaTile tile)
    {
      _currentTile = tile;
      _isPressingTile = false;
      _holdTimer = 0f;
    }

    private void Update()
    {
      if (_currentTile == null)
        return;

      if (_inputService.Click())
        TryStartPressing();

      if (_isPressingTile && _inputService.Hold())
        TickHold();

      if (_inputService.Release())
        StopPressing();
    }

    private void TryStartPressing()
    {
      if (!RaycastHitsCurrentTile())
        return;

      _isPressingTile = true;
      _holdTimer = 0f;
      _metaObserver.TryProgressTile(_currentTile);
    }

    private void TickHold()
    {
      _holdTimer += Time.deltaTime;
      if (_holdTimer < _holdStepInterval)
        return;

      _holdTimer -= _holdStepInterval;
      _metaObserver.TryProgressTile(_currentTile);
    }

    private void StopPressing()
    {
      _isPressingTile = false;
      _holdTimer = 0f;
    }

    private bool RaycastHitsCurrentTile()
    {
      if (_camera == null)
        _camera = Camera.main;

      if (_camera == null)
        return false;

      if (!Physics.Raycast(GetRay(), out var hit, 100f, _tileLayer))
        return false;

      return hit.collider.GetComponentInParent<MetaTile>() == _currentTile;
    }

    private Ray GetRay()
    {
#if UNITY_EDITOR
      return _camera.ScreenPointToRay(Input.mousePosition);
#else
      if (Input.touchCount > 0)
        return _camera.ScreenPointToRay(Input.GetTouch(0).position);
      return _camera.ScreenPointToRay(Input.mousePosition);
#endif
    }
  }
}
