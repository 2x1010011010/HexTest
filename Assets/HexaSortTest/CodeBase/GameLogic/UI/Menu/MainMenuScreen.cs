using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.Menu
{
  public class MainMenuScreen : MonoBehaviour
  {
    [SerializeField, BoxGroup("BUTTONS")] private MainMenuPlayButton _playButton;
    [SerializeField, BoxGroup("BUTTONS")] private MetaButton _metaButton;
    [SerializeField, BoxGroup("BUTTONS")] private ShopButton _shopButton;

    public event Action OnPlayClicked;
    public event Action OnMetaClicked;
    public event Action OnShopClicked;

    private void OnEnable()
    {
      if (_playButton == null)
      {
        Debug.LogError("[MainMenuScreen] _playButton is not assigned in the inspector!");
      }
      else
      {
        _playButton.OnPlayButtonClick += HandlePlayClicked;
      }
	  
      if (_playButton == null)
      {
        Debug.LogError("[MainMenuScreen] _playButton is not assigned in the inspector!");
      }
      else
      {
        _metaButton.OnMetaButtonClick += HandleMetaClicked;
      }

      if (_shopButton == null)
        Debug.LogError("[MainMenuScreen] _shopButton is not assigned in the inspector!");
      else
        _shopButton.OnShopButtonClick += HandleShopClicked;
    }

    private void OnDisable()
    {
      if (_playButton != null)
        _playButton.OnPlayButtonClick -= HandlePlayClicked;

      if (_metaButton != null)
        _metaButton.OnMetaButtonClick -= HandleMetaClicked;

      if (_shopButton != null)
        _shopButton.OnShopButtonClick -= HandleShopClicked;
    }

    private void HandlePlayClicked()
    {
      Debug.Log("[MainMenuScreen] _playClicked");
      OnPlayClicked?.Invoke();
    }

    private void HandleMetaClicked()
    {
      Debug.Log("[MainMenuScreen] _metaClicked");
      OnMetaClicked?.Invoke();
    }

    private void HandleShopClicked()
    {
      Debug.Log("[MainMenuScreen] _shopClicked");
      OnShopClicked?.Invoke();
    }
  }
}