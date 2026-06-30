using HexaSortTest.CodeBase.GameLogic.UI.HUD;
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
      if (_resultPopup == null)
      {
        Debug.LogError("[GameResultSceneInstaller] _resultPopup is not assigned in the inspector!");
        return;
      }

      var registry = Container.Resolve<IGameResultPopupRegistry>();
      registry.Register(_resultPopup);
    }
  }
}