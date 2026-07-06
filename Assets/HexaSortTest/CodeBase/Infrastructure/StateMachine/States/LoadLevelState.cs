using HexaSortTest.CodeBase.GameLogic.GridLogic;
using HexaSortTest.CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States
{
  public class LoadLevelState : IPayloadState<string>
  {
    private readonly SceneLoader _sceneLoader;
    private readonly GameStateMachine _gameStateMachine;
    private readonly IGameFactory _gameFactory;
    private readonly IUIFactory _uiFactory;
    private readonly IPersistentProgressService _progressService;

    public LoadLevelState(
      GameStateMachine gameStateMachine,
      SceneLoader sceneLoader,
      IGameFactory gameFactory,
      IUIFactory uiFactory,
      IPersistentProgressService progressService)
    {
      _gameStateMachine = gameStateMachine;
      _sceneLoader = sceneLoader;
      _gameFactory = gameFactory;
      _uiFactory = uiFactory;
      _progressService = progressService;
    }

    public void Enter(string sceneName)
    {
      _gameFactory.Clear();
      _uiFactory.Clear();
      _sceneLoader.Load(sceneName, OnLoaded);
    }

    public void Exit()
    {
    }

    private void OnLoaded()
    {
      InitGameWorld();
      InformProgressReaders();

      _gameStateMachine.Enter<GameLoopState>();
    }

    private void InformProgressReaders()
    {
      foreach (IProgressReader reader in _gameFactory.ProgressReaders)
        reader.LoadProgress(_progressService.PlayerProgress);
    }

    private void InitGameWorld()
    {
      var mainMenuInstance = _uiFactory.CreateMainMenu();
      var poolInstance = _gameFactory.CreateCellPool();

      int levelIndex = _progressService.PlayerProgress.LevelIndex;

      var gridSpawner = _gameFactory.CreateGridSpawner(poolInstance, mainMenuInstance, levelIndex);
      var gridInstance = gridSpawner.SpawnGrid();
      var gridObserver = gridInstance.GetComponent<GridObserver>();

      var stacksSpawner = _gameFactory.CreateStacksSpawner(poolInstance, gridInstance.GetComponent<HexGrid>());

      _uiFactory.CreateHud(_gameFactory.CurrentLevelConfig.WinCondition, mainMenuInstance, stacksSpawner, gridObserver);

      gridObserver.SetGameResultHandler(_gameStateMachine);
    }

    private void CameraSetup(GameObject target)
    {
    }
  }
}
