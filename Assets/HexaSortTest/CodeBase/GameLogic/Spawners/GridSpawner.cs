using System;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Spawners
{
  public class GridSpawner : MonoBehaviour
  {
    public event Action OnGridSpawned;
    
    private GameObject _grid;
    
    public void Initialize(GameObject grid) => 
      _grid = grid;

    public void SpawnGrid()
    {
      if (_grid == null) return;
      
      Instantiate(_grid, Vector3.zero, Quaternion.identity);
      OnGridSpawned?.Invoke();
    }

    public void Clear() => 
      Destroy(_grid);
  }
}