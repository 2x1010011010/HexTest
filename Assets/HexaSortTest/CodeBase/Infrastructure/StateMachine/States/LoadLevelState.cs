using CodeBase.Infrastructure;
using CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States
{
  public class LoadLevelState : IPayloadState<string>
  {
    private readonly SceneLoader _sceneLoader;
    private readonly StateMachine _stateMachine;
    private readonly IGameFactory _gameFactory;
    private readonly IPersistentProgressService _progressService;

    public LoadLevelState(StateMachine stateMachine, SceneLoader sceneLoader, IGameFactory gameFactory, IPersistentProgressService progressService)
    {
      _stateMachine = stateMachine;
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

      _stateMachine.Enter<GameLoopState>();
    }

    private void InformProgressReaders()
    {
      foreach (IProgressReader reader in _gameFactory.ProgressReaders)
        reader.LoadProgress(_progressService.PlayerProgress);
    }

    private void InitGameWorld()
    {
      var character = _gameFactory.CreatePlayer
      (
        GameObject.FindGameObjectWithTag
            (Constants.SpawnPointTag)
          .transform
      );
      
      character.transform.SetParent(null);

      _gameFactory.CreateHud();
      
      CameraSetup(character);
    }

    private void CameraSetup(GameObject target)
    {

    }
  }
}