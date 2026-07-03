using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.ResultPopup
{
  public class GameResultPopup : UIWindow
  {
    [SerializeField, BoxGroup("VIEWS")] private GameObject _victoryView;
    [SerializeField, BoxGroup("VIEWS")] private GameObject _defeatView;

    [SerializeField, BoxGroup("BUTTONS")] private GameResultContinueButton _continueButton;
    [SerializeField, BoxGroup("BUTTONS")] private GameResultMainMenuButton _mainMenuButton;

    [SerializeField, BoxGroup("ANIMATION")]
    private float _openDuration = 0.3f;

    public event Action OnContinueClicked;
    public event Action OnMainMenuClicked;

    private void OnEnable()
    {
      if (_continueButton == null)
        Debug.LogError("[GameResultPopup] _continueButton is not assigned in the inspector!");
      else
        _continueButton.OnContinueButtonClick += HandleContinueClicked;

      if (_mainMenuButton == null)
        Debug.LogError("[GameResultPopup] _mainMenuButton is not assigned in the inspector!");
      else
        _mainMenuButton.OnMainMenuButtonClick += HandleMainMenuClicked;
    }

    private void OnDisable()
    {
      if (_continueButton != null)
        _continueButton.OnContinueButtonClick -= HandleContinueClicked;

      if (_mainMenuButton != null)
        _mainMenuButton.OnMainMenuButtonClick -= HandleMainMenuClicked;
    }

    public void ShowVictory()
    {
      Debug.Log(
        $"[GameResultPopup] ShowVictory on {gameObject.name}. victoryView={(_victoryView != null)}, defeatView={(_defeatView != null)}");

      if (_victoryView != null) _victoryView.SetActive(true);
      if (_defeatView != null) _defeatView.SetActive(false);
      Open();
    }

    public void ShowDefeat()
    {
      Debug.Log(
        $"[GameResultPopup] ShowDefeat on {gameObject.name}. victoryView={(_victoryView != null)}, defeatView={(_defeatView != null)}");

      if (_victoryView != null) _victoryView.SetActive(false);
      if (_defeatView != null) _defeatView.SetActive(true);
      Open();
    }

    public override void Open()
    {
      Debug.Log($"[GameResultPopup] Open called on {gameObject.name}. activeBefore={gameObject.activeSelf}");
      base.Open();
      transform.localScale = Vector3.zero;
      transform.DOScale(Vector3.one, _openDuration).SetEase(Ease.OutBack);
    }

    public override void Close() =>
      base.Close();

    private void HandleContinueClicked()
    {
      Debug.Log("[GameResultPopup] HandleContinueClicked");
      OnContinueClicked?.Invoke();
    }

    private void HandleMainMenuClicked()
    {
      Debug.Log("[GameResultPopup] HandleMainMenuClicked");
      OnMainMenuClicked?.Invoke();
    }
  }
}
