using System;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public sealed class ExitConfirmationPopup : MonoBehaviour
  {
    [SerializeField] private ExitConfirmButton _confirmButton;
    [SerializeField] private ExitCancelButton _cancelButton;
    public event Action Confirmed;
    public bool IsOpen { get; private set; }
    private bool _confirming;

    private void OnEnable()
    {
      _confirmButton.Clicked += Confirm;
      _cancelButton.Clicked += Cancel;
    }

    private void OnDisable()
    {
      _confirmButton.Clicked -= Confirm;
      _cancelButton.Clicked -= Cancel;
      IsOpen = false;
    }

    public void Show()
    {
      AllowRetry();
      IsOpen = true;
      transform.SetAsLastSibling();
      gameObject.SetActive(true);
    }

    public void Hide()
    {
      IsOpen = false;
      gameObject.SetActive(false);
    }

    public void AllowRetry()
    {
      _confirming = false;
      _confirmButton.Button.interactable = true;
      _cancelButton.Button.interactable = true;
    }

    private void Cancel()
    {
      if (_confirming) return;
      AudioFacade.Instance?.PlayClick();
      Hide();
    }

    private void Confirm()
    {
      if (!IsOpen || _confirming) return;
      _confirming = true;
      _confirmButton.Button.interactable = _cancelButton.Button.interactable = false;
      AudioFacade.Instance?.PlayClick();
      Confirmed?.Invoke();
    }
  }
}
