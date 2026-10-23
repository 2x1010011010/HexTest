using System;
using System.Collections.Generic;
using HexaSortTest.CodeBase.GameLogic.UI.Loading;
using HexaSortTest.CodeBase.Infrastructure.Logging;
using HexaSortTest.CodeBase.Infrastructure.Services.CurrencyService;
using HexaSortTest.CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.Infrastructure.Services.MetaProgressService;
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
      IUIFactory                uiFactory,
      IMetaObjectFactory         metaObjectFactory,
      ICurrencyService           currencyService)
    {
      _states = new Dictionary<Type, IExitState>
      {
        [typeof(BootstrapState)]    = new BootstrapState(this, sceneLoader, curtain),
        [typeof(LoadProgressState)] = new LoadProgressState(this, progressService, saveLoadService),
        [typeof(MainMenuState)]     = new MainMenuState(this, sceneLoader, curtain, uiFactory, progressService),
        [typeof(LoadLevelState)]    = new LoadLevelState(this, sceneLoader, gameFactory, uiFactory, progressService),
        [typeof(GameLoopState)]     = new GameLoopState(this, curtain, uiListenerService, gameFactory, uiFactory),
        [typeof(GameResultState)]   = new GameResultState(this, sceneLoader, uiFactory, progressService, saveLoadService, gameFactory, currencyService),
        [typeof(MetaState)]         = new MetaState(this, sceneLoader, curtain, uiFactory, metaObjectFactory),
        [typeof(ShopState)]         = new ShopState(this, sceneLoader, curtain, uiFactory),
      };
#if UNITY_EDITOR
      GameLogger.Message($"[GameStateMachine] Registered {_states.Count} states.");
#endif
    }
    
    public void Enter<TState>() where TState : class, IState
    {
      IState state = ChangeState<TState>();
#if UNITY_EDITOR
      GameLogger.Message($"[GameStateMachine] Calling {typeof(TState).Name}.Enter().");
#endif
      state.Enter();
    }

    public void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadState<TPayload>
    {
      IPayloadState<TPayload> state = ChangeState<TState>();
#if UNITY_EDITOR
      GameLogger.Message($"[GameStateMachine] Calling {typeof(TState).Name}.Enter() with payload type {typeof(TPayload).Name}.");
#endif
      state.Enter(payload);
    }

    private TState ChangeState<TState>() where TState : class, IExitState
    {
#if UNITY_EDITOR
      GameLogger.Message($"[GameStateMachine] Transition requested: {_currentState?.GetType().Name ?? "None"} -> {typeof(TState).Name}; exiting previous state.");
#endif
      _currentState?.Exit();
#if UNITY_EDITOR
      GameLogger.Message($"[GameStateMachine] Previous state exited; selecting {typeof(TState).Name}.");
#endif
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
