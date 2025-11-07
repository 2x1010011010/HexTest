using System;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Spawners
{
  public class GridSpawner : MonoBehaviour
  {
    private GameObject _grid;
    
    public void Initialize(GameObject grid) => 
      _grid = grid;

    public void SpawnGrid()
    {
      if (_grid == null) return;
      
      Instantiate(_grid, Vector3.zero, Quaternion.identity);
    }

    public void Clear() => 
      Destroy(_grid);
  }
}