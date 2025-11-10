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
    private readonly IPersistentProgressService _progressService;

    public LoadLevelState(GameStateMachine gameStateMachine, SceneLoader sceneLoader, IGameFactory gameFactory, IPersistentProgressService progressService)
    {
      _gameStateMachine = gameStateMachine;
      _sceneLoader = sceneLoader;
      _gameFactory = gameFactory;
      _progressService = progressService;
    }
    
    public void Enter(string sceneName)
    {
      _gameFactory.Clear();
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
      var poolInstance = _gameFactory.CreateCellPool();
      
      var gridSpawner = _gameFactory.CreateGridSpawner(poolInstance);
      var gridInstance = gridSpawner.SpawnGrid();

      _gameFactory.CreateStacksSpawner(poolInstance, gridInstance.GetComponent<HexGrid>());

      _gameFactory.CreateHud();
    }

    private void CameraSetup(GameObject target)
    {

    }
  }
}