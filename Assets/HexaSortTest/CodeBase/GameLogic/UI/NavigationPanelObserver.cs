using HexaSortTest.CodeBase.GameLogic.UI.Menu;
using HexaSortTest.CodeBase.Infrastructure;
using HexaSortTest.CodeBase.Infrastructure.StateMachine;
using HexaSortTest.CodeBase.Infrastructure.StateMachine.States;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace HexaSortTest.CodeBase.GameLogic.UI
{
  public sealed class NavigationPanelObserver : MonoBehaviour
  {
    [SerializeField, BoxGroup("BUTTONS")] private ShopButton _shopButton;
    [SerializeField, BoxGroup("BUTTONS")] private HomeButton _homeButton;
    [SerializeField, BoxGroup("BUTTONS")] private MetaButton _metaButton;

    [SerializeField, BoxGroup("BUTTON SIZES")] private Vector2 _activeButtonSize = new Vector2(300f, 300f);
    [SerializeField, BoxGroup("BUTTON SIZES")] private Vector2 _inactiveButtonSize = new Vector2(150f, 150f);

    private GameStateMachine _gameStateMachine;
    private string _currentScene;
    private bool _isNavigating;

    // Called after the scene and its factory-created UI are ready. This also
    // finds panels nested in UI prefabs and panels that start inactive.
    public static void InitializeInScene(GameStateMachine stateMachine, string sceneName)
    {
      foreach (var panel in Object.FindObjectsByType<NavigationPanelObserver>(
                 FindObjectsInactive.Include, FindObjectsSortMode.None))
      {
        if (panel.gameObject.scene.name == sceneName)
          panel.Initialize(stateMachine, sceneName);
      }
    }

    public void Initialize(GameStateMachine stateMachine, string currentScene)
    {
      _gameStateMachine = stateMachine;
      _currentScene = currentScene;
      _isNavigating = false;
      RefreshButtons();
    }

    private void OnEnable()
    {
      if (_shopButton != null) _shopButton.OnShopButtonClick += OpenShop;
      if (_homeButton != null) _homeButton.OnHomeButtonClick += OpenHome;
      if (_metaButton != null) _metaButton.OnMetaButtonClick += OpenMeta;
      RefreshButtons();
    }

    private void OnDisable()
    {
      if (_shopButton != null) _shopButton.OnShopButtonClick -= OpenShop;
      if (_homeButton != null) _homeButton.OnHomeButtonClick -= OpenHome;
      if (_metaButton != null) _metaButton.OnMetaButtonClick -= OpenMeta;
    }

    private void OpenShop() => Navigate<ShopState>(Constants.ShopScene);
    private void OpenHome() => Navigate<MainMenuState>(Constants.MainMenuScene);
    private void OpenMeta() => Navigate<MetaState>(Constants.MetaScene);

    private void Navigate<TState>(string sceneName) where TState : class, IState
    {
      if (_gameStateMachine == null || _isNavigating || _currentScene == sceneName)
        return;

      _isNavigating = true;
      RefreshButtons();
      _gameStateMachine.Enter<TState>();
    }

    private void RefreshButtons()
    {
      bool ready = _gameStateMachine != null && !_isNavigating;
      string scene = string.IsNullOrEmpty(_currentScene) ? gameObject.scene.name : _currentScene;
      RefreshButton(_shopButton, scene == Constants.ShopScene, ready);
      RefreshButton(_homeButton, scene == Constants.MainMenuScene, ready);
      RefreshButton(_metaButton, scene == Constants.MetaScene, ready);
    }

    private void RefreshButton(ButtonBase component, bool selected, bool ready)
    {
      if (component == null || component.Button == null) return;
      var button = component.Button;

      // The current scene stays non-clickable without the default disabled tint.
      var colors = button.colors;
      colors.disabledColor = colors.normalColor;
      colors.selectedColor = colors.normalColor;
      button.colors = colors;
      button.interactable = ready && !selected;

      var rect = button.transform as RectTransform;
      if (rect == null) return;
      Vector2 size = selected ? _activeButtonSize : _inactiveButtonSize;
      size.x = Mathf.Max(1f, size.x);
      size.y = Mathf.Max(1f, size.y);
      rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
      rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

      // Keep the requested size when the panel uses a LayoutGroup.
      var layout = rect.GetComponent<LayoutElement>();
      if (layout == null && rect.parent != null && rect.parent.GetComponent<LayoutGroup>() != null)
        layout = rect.gameObject.AddComponent<LayoutElement>();
      if (layout != null)
      {
        layout.minWidth = layout.preferredWidth = size.x;
        layout.minHeight = layout.preferredHeight = size.y;
        layout.flexibleWidth = layout.flexibleHeight = 0f;
      }
    }
  }
}
