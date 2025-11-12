using HexaSortTest.CodeBase.Infrastructure.Services.UIService;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.MainMenu
{
  public class MainMenuObserver : UIWindow
  {
    [SerializeField, BoxGroup("BUTTONS")] private RestartLevelButton _restartLevelButton;

    private IUIListenerService _uiService;

    public void Initialize(IUIListenerService uiService)
    {
      _uiService = uiService;
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