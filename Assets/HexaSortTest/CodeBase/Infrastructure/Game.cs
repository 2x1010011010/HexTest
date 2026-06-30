using HexaSortTest.CodeBase.GameLogic.UI.Loading;
using HexaSortTest.CodeBase.Infrastructure.Services;
using HexaSortTest.CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.Infrastructure.Services.GameResultService;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using HexaSortTest.CodeBase.Infrastructure.Services.SaveAndLoadService;
using HexaSortTest.CodeBase.Infrastructure.Services.UIService;
using HexaSortTest.CodeBase.Infrastructure.StateMachine;
using Zenject;

namespace HexaSortTest.CodeBase.Infrastructure
{
  public sealed class Game
  {
    public readonly GameStateMachine StateMachine;

    [Inject]
    public Game(
      ICoroutineRunner coroutineRunner,
      LoadingCurtain curtain,
      IPersistentProgressService progressService,
      ISaveLoadService saveLoadService,
      IGameFactory gameFactory,
      IUIListenerService uiListenerService,
      IGameResultPopupRegistry popupRegistry
    ) =>
      StateMachine = new GameStateMachine(
        new SceneLoader(coroutineRunner),
        curtain,
        progressService,
        saveLoadService,
        gameFactory,
        uiListenerService,
        popupRegistry);
  }
}