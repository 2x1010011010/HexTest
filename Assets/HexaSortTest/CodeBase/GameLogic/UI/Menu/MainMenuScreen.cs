using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.Menu
{
  public class MainMenuScreen : MonoBehaviour
  {
    [SerializeField, BoxGroup("BUTTONS")] private MainMenuPlayButton _playButton;
    [SerializeField, BoxGroup("BUTTONS")] private MetaButton _metaButton;

    public event Action OnPlayClicked;
    public event Action OnMetaClicked;

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
    }

    private void OnDisable()
    {
      if (_playButton != null)
        _playButton.OnPlayButtonClick -= HandlePlayClicked;

      if (_metaButton != null)
        _metaButton.OnMetaButtonClick -= HandleMetaClicked;
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
  }
}