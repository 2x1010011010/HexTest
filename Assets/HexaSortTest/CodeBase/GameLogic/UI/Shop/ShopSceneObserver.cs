using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.Shop
{
  public class ShopSceneObserver : MonoBehaviour
  {
    [SerializeField, BoxGroup("BUTTONS")] private ShopExitButton _exitButton;

    public event Action OnExitRequested;

    private void OnEnable()
    {
      if (_exitButton != null)
        _exitButton.OnExitButtonClick += HandleExitClicked;
      else
        Debug.LogError("[ShopSceneObserver] _exitButton is not assigned in the inspector!");
    }

    private void OnDisable()
    {
      if (_exitButton != null)
        _exitButton.OnExitButtonClick -= HandleExitClicked;
    }

    private void HandleExitClicked() =>
      OnExitRequested?.Invoke();
  }
}