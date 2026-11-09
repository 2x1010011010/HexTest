using HexaSortTest.CodeBase.GameLogic.UI;
using HexaSortTest.CodeBase.GameLogic.UI.Loading;
using HexaSortTest.CodeBase.GameLogic.UI.Meta;
using HexaSortTest.CodeBase.Infrastructure.Logging;
using HexaSortTest.CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.Infrastructure.Services.MetaProgressService;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States
{
  public class MetaState : IState
  {
    private readonly GameStateMachine _gameStateMachine;
    private readonly SceneLoader _sceneLoader;
    private readonly LoadingCurtain _loadingCurtain;
    private readonly IUIFactory _uiFactory;
    private readonly IMetaObjectFactory _metaObjectFactory;

    private MetaUIObserver _metaUI;

    public MetaState(
      GameStateMachine gameStateMachine,
      SceneLoader sceneLoader,
      LoadingCurtain loadingCurtain,
      IUIFactory uiFactory,
      IMetaObjectFactory metaObjectFactory)
    {
      _gameStateMachine = gameStateMachine;
      _sceneLoader = sceneLoader;
      _loadingCurtain = loadingCurtain;
      _uiFactory = uiFactory;
      _metaObjectFactory = metaObjectFactory;
    }

    public void Enter()
    {
      _loadingCurtain.Show();
      _sceneLoader.Load(Constants.MetaScene, onLoaded: SpawnMetaUI);
    }

    public void Exit()
    {
      _metaUI = null;
      _metaObjectFactory.Clear();
      _uiFactory.Clear();
    }

    private void SpawnMetaUI()
    {
      _loadingCurtain.Hide();

      _metaUI = _uiFactory.CreateMetaUI();
      NavigationPanelObserver.InitializeInScene(_gameStateMachine, Constants.MetaScene);

      if (_metaUI == null)
      {
#if UNITY_EDITOR
        GameLogger.Error("[MetaState] Failed to spawn MetaUIObserver. " +
                       "Check that AssetPaths.MetaUIPrefab points to a valid prefab under Resources/.");
#endif
        return;
      }

#if UNITY_EDITOR
      GameLogger.Message("[MetaState] UI created.");
#endif
    }

  }
}
