using System;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Spawners
{
  public class GridSpawner : MonoBehaviour
  {
    private GameObject _grid;
    
    public void Initialize(GameObject grid) => 
      _grid = grid;

    public GameObject SpawnGrid()
    {
      if (_grid == null) return null;
      
      var spawned = Instantiate(_grid, Vector3.zero, Quaternion.identity);
      return spawned;
    }

    public void Clear() => 
      Destroy(_grid);
  }
}