using HexaSortTest.CodeBase.GameLogic.UI.Loading;
using HexaSortTest.CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.Infrastructure.Services.UIService;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States
{
  public class GameLoopState : IState
  {
    private readonly GameStateMachine _gameStateMachine;
    private readonly LoadingCurtain _loadingCurtain;
    private readonly IUIListenerService _uiListenerService;
    private readonly IGameFactory _gameFactory;

    public GameLoopState(GameStateMachine gameStateMachine, LoadingCurtain curtain, IUIListenerService uiListenerService, IGameFactory gameFactory)
    {
      _gameStateMachine = gameStateMachine;
      _loadingCurtain = curtain;
      _uiListenerService = uiListenerService;
      _gameFactory = gameFactory;
      
    }

    public void Enter()
    {
      _uiListenerService.ActionRequired += ClearScene;
      _loadingCurtain.Hide();
    }

    public void Exit()
    {
      _uiListenerService.ActionRequired -= ClearScene;
    }

    private void ClearScene()
    {
      Debug.Log("$\"[GameLoopState] ClearScene() called. Instances before clear: {_gameFactory.InstancesCount}");
      _loadingCurtain.Show();
      _gameFactory.Clear();
      _gameStateMachine.Enter<LoadLevelState, string>(Constants.GameScene);
    }
  }
}