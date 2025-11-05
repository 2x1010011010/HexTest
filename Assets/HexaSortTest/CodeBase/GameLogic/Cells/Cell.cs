using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Cells
{
  public class Cell : MonoBehaviour
  {
    [SerializeField] private bool _isSpawner;
    public bool IsSpawner => _isSpawner;

    public void SetSpawner(bool isSpawner) => 
      _isSpawner = isSpawner;
  }
}