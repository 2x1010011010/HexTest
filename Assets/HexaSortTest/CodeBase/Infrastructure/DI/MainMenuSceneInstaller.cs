using HexaSortTest.CodeBase.GameLogic.UI.Menu;
using HexaSortTest.CodeBase.Infrastructure.Services.MainMenuService;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace HexaSortTest.CodeBase.Infrastructure.DI
{
  public class MainMenuSceneInstaller : MonoInstaller
  {
    [SerializeField, BoxGroup("SCENE REFERENCES")]
    private MainMenuScreen _mainMenuScreen;

    public override void InstallBindings()
    {
      if (_mainMenuScreen == null)
      {
        Debug.LogError("[MainMenuSceneInstaller] _mainMenuScreen is not assigned in the inspector!");
        return;
      }

      var registry = Container.Resolve<IMainMenuRegistry>();
      registry.Register(_mainMenuScreen);
    }
  }
}