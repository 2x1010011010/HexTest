using HexaSortTest.CodeBase.Infrastructure.Services.LivesService;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using HexaSortTest.CodeBase.Infrastructure.Logging;
using HexaSortTest.CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States
{
  public class LoadLevelState : IPayloadState<string>
  {
    private readonly LivesService _lives;
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
      IPersistentProgressService progressService,
      LivesService lives)
    {
      _gameStateMachine = gameStateMachine;
      _sceneLoader = sceneLoader;
      _gameFactory = gameFactory;
      _uiFactory = uiFactory;
      _progressService = progressService;
      _lives = lives;
    }

    public void Enter(string sceneName)
    {
      _lives.Refresh();
      if (_lives.Lives <= 0)
      {
        _gameStateMachine.Enter<MainMenuState>();
        return;
      }
#if UNITY_EDITOR
      GameLogger.Message($"[LoadLevelState] Preparing scene {sceneName}, level index {_progressService.PlayerProgress.LevelIndex}; clearing previous objects.");
#endif
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

      _gameStateMachine.BeginLevelAttempt();
      _gameStateMachine.Enter<GameLoopState>();
    }

    private void InformProgressReaders()
    {
#if UNITY_EDITOR
      GameLogger.Message($"[LoadLevelState] Restoring progress for {_gameFactory.ProgressReaders.Count} readers.");
#endif
      foreach (IProgressReader reader in _gameFactory.ProgressReaders)
        reader.LoadProgress(_progressService.PlayerProgress);
#if UNITY_EDITOR
      GameLogger.Message("[LoadLevelState] Progress readers restored.");
#endif
    }

    private void InitGameWorld()
    {
#if UNITY_EDITOR
      GameLogger.Message("[LoadLevelState] Creating UI, tile pool and grid.");
#endif
      var mainMenuInstance = _uiFactory.CreateMainMenu();
      var poolInstance = _gameFactory.CreateCellPool();

      int levelIndex = _progressService.PlayerProgress.LevelIndex;

      var gridSpawner = _gameFactory.CreateGridSpawner(poolInstance, mainMenuInstance, levelIndex);
      var gridInstance = gridSpawner.SpawnGrid();
      var gridObserver = gridInstance.GetComponent<GridObserver>();

      var stacksSpawner = _gameFactory.CreateStacksSpawner(poolInstance, gridInstance.GetComponent<HexGrid>());

      var boosterPurchasePopup = _uiFactory.CreateBoosterPurchasePopup();

      _uiFactory.CreateHud(_gameFactory.CurrentLevelConfig.WinCondition, mainMenuInstance, stacksSpawner, gridObserver, boosterPurchasePopup);

      gridObserver.SetGameResultHandler(_gameStateMachine);
#if UNITY_EDITOR
      GameLogger.Message($"[LoadLevelState] World ready; level index {levelIndex}, win condition {_gameFactory.CurrentLevelConfig.WinCondition}.");
#endif
    }

    private void CameraSetup(GameObject target)
    {
    }
  }
}
