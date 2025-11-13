using HexaSortTest.CodeBase.Infrastructure.Services;
using HexaSortTest.CodeBase.Infrastructure.Services.UIService;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.MainMenu
{
  public class MainMenuObserver : UIWindow
  {
    [SerializeField, BoxGroup("BUTTONS")] private RestartLevelButton _restartLevelButton;

    private IUIListenerService _uiService;

    public void Start()
    {
      _uiService = ServiceLocator.Container.Single<IUIListenerService>();
    }

    private void OnEnable() => 
      _restartLevelButton.OnRestartLevelButtonClick += RestartLevel;
    
    private void OnDisable() => 
      _restartLevelButton.OnRestartLevelButtonClick -= RestartLevel;

    private void RestartLevel()
    {
      _uiService.NotifyActionRequired();
      Close();
    }
  }
}