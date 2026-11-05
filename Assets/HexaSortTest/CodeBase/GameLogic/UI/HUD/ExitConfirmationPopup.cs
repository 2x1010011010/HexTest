using System;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;
using UnityEngine;
using UnityEngine.UI;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public sealed class ExitConfirmationPopup : MonoBehaviour
  {
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;
    public event Action Confirmed;
    public bool IsOpen { get; private set; }
    private bool _confirming;

    private void OnEnable()
    {
      _confirmButton.onClick.AddListener(Confirm);
      _cancelButton.onClick.AddListener(Cancel);
    }

    private void OnDisable()
    {
      _confirmButton.onClick.RemoveListener(Confirm);
      _cancelButton.onClick.RemoveListener(Cancel);
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
      _confirmButton.interactable = true;
      _cancelButton.interactable = true;
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
      _confirmButton.interactable = _cancelButton.interactable = false;
      AudioFacade.Instance?.PlayClick();
      Confirmed?.Invoke();
    }
  }
}
