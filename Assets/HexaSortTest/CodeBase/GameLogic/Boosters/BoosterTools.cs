using UnityEngine;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using Sirenix.OdinInspector;

namespace HexaSortTest.CodeBase.GameLogic.Boosters
{
  public class BoosterTools : MonoBehaviour
  {
    [SerializeField, BoxGroup("SETUP")] private LayerMask _stackLayer;
    
    private Camera _camera;
    private IBooster _currentBooster;
    private bool _isBoosterActive;

    private void Awake()
    {
      if (_camera == null)
        _camera = Camera.main;
    }

    private void Update()
    {
      if (!_isBoosterActive) return;

#if UNITY_EDITOR
      if (Input.GetMouseButtonDown(0))
      {
        TryApplyBoosterAtScreenPoint(Input.mousePosition);
      }

      if (Input.GetMouseButtonUp(0))
        DeactivateBooster();
#else
      if (Input.touchCount > 0 && Input.GetTouch(0).phase == UnityEngine.TouchPhase.Began)
      {
        TryApplyBoosterAtScreenPoint(Input.GetTouch(0).position);
      }
#endif
    }

    private void TryApplyBoosterAtScreenPoint(Vector2 screenPoint)
    {
      var ray = _camera.ScreenPointToRay(screenPoint);
      if (Physics.Raycast(ray, out var hit, 100f, _stackLayer))
      {
        var foundStack = hit.collider.GetComponentInParent<Stack>();
        if (foundStack != null)
        {
          _currentBooster?.BoosterAction(foundStack);
        }
      }
    }

    public void ActivateBooster(IBooster booster)
    {
      _currentBooster = booster;
      _isBoosterActive = true;
    }

    public void DeactivateBooster()
    {
      _currentBooster = null;
      _isBoosterActive = false;
    }
  }
}