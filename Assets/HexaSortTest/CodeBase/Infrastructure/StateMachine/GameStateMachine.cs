using System;
using System.Collections.Generic;
using HexaSortTest.CodeBase.GameLogic.UI.Loading;
using HexaSortTest.CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using HexaSortTest.CodeBase.Infrastructure.Services.SaveAndLoadService;
using HexaSortTest.CodeBase.Infrastructure.Services.UIService;
using HexaSortTest.CodeBase.Infrastructure.StateMachine.States;

namespace HexaSortTest.CodeBase.Infrastructure.StateMachine
{
  public class GameStateMachine
  {
    private readonly Dictionary<Type, IExitState> _states;
    private IExitState _currentState;
    
    public GameStateMachine(
      SceneLoader               sceneLoader,
      LoadingCurtain            curtain,
      IPersistentProgressService progressService,
      ISaveLoadService          saveLoadService,
      IGameFactory              gameFactory,
      IUIListenerService        uiListenerService,
      IMainMenuFactory          menuFactory,
      IGameResultFactory        resultFactory)
    {
      _states = new Dictionary<Type, IExitState>
      {
        [typeof(BootstrapState)]    = new BootstrapState(this, sceneLoader, curtain),
        [typeof(LoadProgressState)] = new LoadProgressState(this, progressService, saveLoadService),
        [typeof(MainMenuState)]     = new MainMenuState(this, sceneLoader, curtain, menuFactory, progressService),
        [typeof(LoadLevelState)]    = new LoadLevelState(this, sceneLoader, gameFactory, progressService),
        [typeof(GameLoopState)]     = new GameLoopState(this, curtain, uiListenerService, gameFactory),
        [typeof(GameResultState)]   = new GameResultState(this, sceneLoader, resultFactory, progressService, saveLoadService),
      };
    }
    
    public void Enter<TState>() where TState : class, IState
    {
      IState state = ChangeState<TState>();
      state.Enter();
    }

    public void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadState<TPayload>
    {
      IPayloadState<TPayload> state = ChangeState<TState>();
      state.Enter(payload);
    }

    private TState ChangeState<TState>() where TState : class, IExitState
    {
      _currentState?.Exit();
      TState state = GetState<TState>();
      _currentState = state;
      return state;  
    }

    private TState GetState<TState>() where TState : class, IExitState
    {
      return _states[typeof(TState)] as TState;
    }
  }
}