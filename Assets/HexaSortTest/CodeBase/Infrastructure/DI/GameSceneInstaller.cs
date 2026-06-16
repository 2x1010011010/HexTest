using HexaSortTest.CodeBase.GameLogic.Spawners;
using HexaSortTest.CodeBase.GameLogic.UI.HUD;
using HexaSortTest.CodeBase.GameLogic.UI.MainMenu;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace HexaSortTest.CodeBase.Infrastructure.DI
{
  public class GameSceneInstaller : MonoInstaller
  {
    [SerializeField, BoxGroup("SCENE REFERENCES")] private HudObserver _hudObserver;
    [SerializeField, BoxGroup("SCENE REFERENCES")] private MainMenuObserver _mainMenuObserver;
    [SerializeField, BoxGroup("SCENE REFERENCES")] private StacksSpawner _stacksSpawner;

    public override void InstallBindings()
    {
      if (_hudObserver != null)
        Container
          .Bind<HudObserver>()
          .FromInstance(_hudObserver)
          .AsSingle();

      if (_mainMenuObserver != null)
        Container
          .Bind<MainMenuObserver>()
          .FromInstance(_mainMenuObserver)
          .AsSingle();

      if (_stacksSpawner != null)
        Container
          .Bind<StacksSpawner>()
          .FromInstance(_stacksSpawner)
          .AsSingle();
    }
  }
}