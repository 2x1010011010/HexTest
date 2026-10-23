using HexaSortTest.CodeBase.GameLogic.UI.ResultPopup;
using HexaSortTest.CodeBase.Infrastructure.Logging;
using HexaSortTest.CodeBase.Infrastructure.Services.GameResultService;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace HexaSortTest.CodeBase.Infrastructure.DI
{
  public class GameResultSceneInstaller : MonoInstaller
  {
    [SerializeField, BoxGroup("SCENE REFERENCES")]
    private GameResultPopup _resultPopup;

    public override void InstallBindings()
    {
#if UNITY_EDITOR
      GameLogger.Message("[DI][GameResultSceneInstaller] Installing bindings.");
#endif
      if (_resultPopup == null)
      {
#if UNITY_EDITOR
        GameLogger.Error("[GameResultSceneInstaller] _resultPopup is not assigned in the inspector!");
#endif
        return;
      }

      var registry = Container.Resolve<IGameResultPopupRegistry>();
      registry.Register(_resultPopup);
#if UNITY_EDITOR
      GameLogger.Message("[DI][GameResultSceneInstaller] Result popup registered.");
#endif
#if UNITY_EDITOR
      GameLogger.Message("[DI][GameResultSceneInstaller] Binding installation complete.");
#endif
    }
  }
}