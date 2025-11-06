using HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Cells
{
  public class Cell : MonoBehaviour, IPoolable
  {
    [SerializeField] private bool _isSpawner;
    public bool IsSpawner => _isSpawner;

    public void SetSpawner(bool isSpawner) => 
      _isSpawner = isSpawner;

    public void OnSpawnedFromPool()
    {
    }

    public void OnReturnedToPool()
    {
    }
  }
}