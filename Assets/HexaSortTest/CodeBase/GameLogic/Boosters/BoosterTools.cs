using UnityEngine;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using Sirenix.OdinInspector;

namespace HexaSortTest.CodeBase.GameLogic.Boosters
{
  public class BoosterTools : MonoBehaviour
  {
    [SerializeField, BoxGroup("BOOSTERS")] private HammerBooster _hammerBooster;
    [SerializeField, BoxGroup("BOOSTERS")] private HandBooster _handBooster;
    [SerializeField, BoxGroup("SETUP")] private LayerMask _stackLayer;
    
    private Camera _camera;
    private IBooster _currentBooster;
    private bool _isBoosterActive;
    private bool _isBoosterApplied;

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
      
      if (_isBoosterApplied && Input.GetMouseButtonUp(0))
        DeactivateBooster();
#else
      if (Input.touchCount > 0 && Input.GetTouch(0).phase == UnityEngine.TouchPhase.Began)
      {
        TryApplyBoosterAtScreenPoint(Input.GetTouch(0).position);
      }
      
      if (Input.touchCount > 0 && Input.GetTouch(0).phase == UnityEngine.TouchPhase.End)
      {
        DeactivateBooster();
      }
#endif
    }

    private void TryApplyBoosterAtScreenPoint(Vector2 screenPoint)
    {
      Debug.Log("Trying to apply booster");
      var ray = _camera.ScreenPointToRay(screenPoint);
      if (Physics.Raycast(ray, out var hit, 100f, _stackLayer))
      {
        Debug.Log("Layer hit:");
        var foundStack = hit.collider.GetComponentInParent<Stack>();
        if (foundStack != null)
        {
          Debug.Log("Stack found");
          _currentBooster?.BoosterAction(foundStack);
          _isBoosterApplied = true;
        }
      }
    }

    public void ActivateBooster(IBooster booster)
    {
      _currentBooster = booster;
      _isBoosterActive = true;
      _isBoosterApplied = false;
    }

    public void DeactivateBooster()
    {
      _currentBooster = null;
      _isBoosterActive = false;
    }
  }
}