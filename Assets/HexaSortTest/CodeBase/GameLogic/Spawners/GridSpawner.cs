using HexaSortTest.CodeBase.GameLogic.GridLogic;
using HexaSortTest.CodeBase.GameLogic.UI.MainMenu;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Spawners
{
  public class GridSpawner : MonoBehaviour
  {
    private GameObject _grid;
    private MainMenuObserver _mainMenu;
    private GameObject _spawned;
    
    public void Initialize(GameObject grid) => 
      _grid = grid;

    public GameObject SpawnGrid()
    {
      if (_grid == null) return null;
      
      var spawned = Instantiate(_grid, Vector3.zero, Quaternion.identity, transform);
      _spawned = spawned;
      spawned.GetComponent<GridObserver>().SetMainMenu(_mainMenu);
      return spawned;
    }

    public void Clear() => 
      Destroy(_spawned);

    public void SetMainMenu(MainMenuObserver mainMenu) => 
      _mainMenu = mainMenu;
  }
}