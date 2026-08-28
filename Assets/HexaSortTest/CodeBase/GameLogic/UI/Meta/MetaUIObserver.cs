using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.Meta
{
  public class MetaUIObserver : MonoBehaviour
  {
    [SerializeField, BoxGroup("LIST SETUP"), Tooltip("The ScrollBox toggled open/closed by the list button.")]
    private GameObject _listPanel;

    [SerializeField, BoxGroup("BUTTONS")] private MetaSwitchButton _switchButton;
    [SerializeField, BoxGroup("BUTTONS")] private MetaExitButton _exitButton;

    public event Action OnExitRequested;

    private bool _isListOpen;

    private void Awake()
    {
      ShowList();
    }

    private void OnEnable()
    {
      if (_exitButton != null)
        _exitButton.OnExitButtonClick += HandleExitClicked;
      else
        Debug.LogError("[MetaUIObserver] _exitButton is not assigned in the inspector!");

      if (_switchButton != null)
        _switchButton.OnSwitchButtonClick += HandleSwitchClicked;
      else
        Debug.LogError("[MetaUIObserver] _switchButton is not assigned in the inspector!");
    }

    private void OnDisable()
    {
      if (_exitButton != null)
        _exitButton.OnExitButtonClick -= HandleExitClicked;

      if (_switchButton != null)
        _switchButton.OnSwitchButtonClick -= HandleSwitchClicked;
    }

    private void HandleSwitchClicked()
    {
      if (_isListOpen)
        HideList();
      else
        ShowList();
    }

    private void ShowList()
    {
      _isListOpen = true;

      if (_listPanel != null)
        _listPanel.SetActive(true);
      else
        Debug.LogError("[MetaUIObserver] _listPanel is not assigned in the inspector! The ScrollBox cannot be shown.");
    }

    private void HideList()
    {
      _isListOpen = false;

      if (_listPanel != null)
        _listPanel.SetActive(false);
    }

    private void HandleExitClicked() =>
      OnExitRequested?.Invoke();
  }
}