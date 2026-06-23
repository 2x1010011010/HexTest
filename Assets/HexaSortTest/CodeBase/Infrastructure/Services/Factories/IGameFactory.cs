using System.Collections.Generic;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using HexaSortTest.CodeBase.GameLogic.Spawners;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using HexaSortTest.CodeBase.GameLogic.UI.MainMenu;
using HexaSortTest.CodeBase.GameLogic.UI.ResultPopup;
using HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.Factories
{
  public interface IGameFactory : IFactory
  {
    public ObjectPool<StackTile> CreateCellPool();
    public GridSpawner CreateGridSpawner(ObjectPool<StackTile> pool, MainMenuObserver mainMenu, int levelIndex);
    void CreateStacksSpawner(ObjectPool<StackTile> pool, HexGrid grid);
    void CreateHud(MainMenuObserver mainMenu, GridObserver gridObserver);
    public MainMenuObserver CreateMainMenu();
    public GameResultPopup CreateGameResultPopup();
    List<IProgressReader> ProgressReaders { get; }
    List<IProgressSaver> ProgressSavers { get; }
    void Clear();
  }
}