using System.Collections.Generic;
using System.Linq;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.GridLogic
{
  public class GridObserver : MonoBehaviour
  {
    [SerializeField, BoxGroup("SETUP")] private HexGrid _grid;
    
    private readonly HashSet<Stack> _stacksOnGrid = new();

    public void Init(HexGrid grid) => _grid = grid;

    private void Start()
    {
      foreach (var cell in _grid.Cells)
      {
        var stack = cell.GetComponentInChildren<Stack>();
        if (stack == null) continue;
        AddStack(stack);
      }

      foreach (var stack in _stacksOnGrid)
        Debug.Log($"Spawned stack: {stack?.GetHashCode()}");
      
      CheckStacks();
    }
    
    private void Update()
    {
      if (Input.GetMouseButtonUp(0))
      {
        if (!IsStackPlaced()) return;
        CheckStacks();
      }
    }

    private void AddStack(Stack stack) => 
      _stacksOnGrid.Add(stack);

    private void RemoveStack(Stack stack) => 
      _stacksOnGrid.Remove(stack);

    private bool IsStackPlaced()
    {
      foreach (var cell in _grid.Cells)
      {
        var stack = cell.GetComponentInChildren<Stack>();
        if (stack== null) continue;
        if (_stacksOnGrid.Contains(stack)) continue;
        AddStack(stack);
        Debug.Log("Stack placed");
        return true;
      }
      Debug.Log("Stack not placed");
      return false;
    }

    private void CheckStacks()
    {
      foreach (var neighbors in _grid.Cells.Select(cell => GetNeighbors(cell)))
      {
        foreach (var neighborStack in neighbors
                   .Select(neighbor => neighbor.GetComponentInChildren<Stack>())
                   .Where(neighborStack => neighborStack != null))
        {
          Debug.Log(neighborStack.GetLastCellColor());
        }
      }
    }
    
    private List<Cell> GetNeighbors(Cell cell)
    {
      LayerMask layerMask = 1 << cell.gameObject.layer;
      var neighbors = Physics.OverlapSphere(cell.transform.position, 5, layerMask).Select(hit => hit.GetComponent<Cell>()).ToList();
      neighbors.Remove(cell);
      return neighbors;
    }
  }
}