using System.Collections.Generic;
using System.Linq;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.GridLogic
{
  public class GridObserver : MonoBehaviour
  {
    [SerializeField, BoxGroup("SETUP")] private HexGrid _grid;
    
    private HashSet<Stack> _stacksOnGrid = new();

    public void Init(HexGrid grid) => _grid = grid;

    private void Awake()
    {
      foreach (var cell in _grid.Cells.Where(cell => cell.IsSpawner)) 
        AddStack(cell.GetComponentInChildren<Stack>());
    }

    private void AddStack(Stack stack) => 
      _stacksOnGrid.Add(stack);

    private void RemoveStack(Stack stack) => 
      _stacksOnGrid.Remove(stack);

    private void CheckStacks()
    {
    }
  }
}