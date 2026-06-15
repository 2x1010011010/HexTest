using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.Spawners;
using UnityEngine;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using Sirenix.OdinInspector;

namespace HexaSortTest.CodeBase.GameLogic.Boosters
{
  public class BoosterTools : MonoBehaviour
  {
    [SerializeField, BoxGroup("BOOSTERS")] private HammerBooster _hammerBooster;
    [SerializeField, BoxGroup("BOOSTERS")] private HandBooster _handBooster;
    [SerializeField, BoxGroup("BOOSTERS")] private RespawnBooster _respawnBooster;
    [SerializeField, BoxGroup("SETUP")] private LayerMask _stackLayer;

    private Camera _camera;
    private IBooster _currentBooster;
    private StacksSpawner _stacksSpawner;
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

      if (_currentBooster.GetType() == typeof(RespawnBooster))
      {
        TryApplyRespawnBooster();
        return;
      }

      if (Input.GetMouseButtonDown(0))
      {
        TryApplyBoosterAtScreenPoint(Input.mousePosition);
      }

      if (_isBoosterApplied && Input.GetMouseButtonUp(0))
        DeactivateBooster();
    }

    private void TryApplyBoosterAtScreenPoint(Vector2 screenPoint)
    {
      Debug.Log("Trying to apply booster");
      var ray = _camera.ScreenPointToRay(screenPoint);
      if (Physics.Raycast(ray, out var hit, 100f, _stackLayer))
      {
        Debug.Log("Layer hit:");
        var foundStack = hit.collider.GetComponentInParent<Stack>();
        if (foundStack == null) return;
        if (foundStack.GetComponentInParent<Cell>() == null) return;
        
        Debug.Log("Stack found");
        _currentBooster?.BoosterAction(foundStack);
        _isBoosterApplied = true;
      }
    }

    private async void TryApplyRespawnBooster()
    {
      var spawnPoints = _stacksSpawner.SpawnPoints;

      foreach (var spawnPoint in spawnPoints)
      {
        var stack = spawnPoint.gameObject.GetComponentInChildren<Stack>();
        if (stack == null) continue;
        await _currentBooster.BoosterAction(stack);
        _stacksSpawner.StackParentChanged(stack);
      }

      _isBoosterApplied = true;
      DeactivateBooster();
      //_stacksSpawner.ClearSpawn();
    }

    public void SetSpawner(StacksSpawner spawner) =>
      _stacksSpawner = spawner;

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