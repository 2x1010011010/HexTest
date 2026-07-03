using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.Menu
{
  public class MainMenuScreen : MonoBehaviour
  {
    [SerializeField, BoxGroup("BUTTONS")] private MainMenuPlayButton _playButton;

    public event Action OnPlayClicked;

    private void OnEnable()
    {
      if (_playButton == null)
      {
        Debug.LogError("[MainMenuScreen] _playButton is not assigned in the inspector!");
        return;
      }

      _playButton.OnPlayButtonClick += HandlePlayClicked;
    }

    private void OnDisable()
    {
      if (_playButton == null)
        return;

      _playButton.OnPlayButtonClick -= HandlePlayClicked;
    }

    private void HandlePlayClicked() =>
      OnPlayClicked?.Invoke();
  }
}