using System;
using DG.Tweening;
using HexaSortTest.CodeBase.GameLogic.UI.ResultPopup;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public class GameResultPopup : UIWindow
  {
    [SerializeField, BoxGroup("VIEWS")] private GameObject _victoryView;
    [SerializeField, BoxGroup("VIEWS")] private GameObject _defeatView;

    [SerializeField, BoxGroup("BUTTON")] private GameResultContinueButton _continueButton;

    [SerializeField, BoxGroup("ANIMATION")]
    private float _openDuration = 0.3f;

    public event Action OnContinueClicked;

    private void OnEnable()
    {
      if (_continueButton == null)
      {
        Debug.LogError("[GameResultPopup] _continueButton is not assigned in the inspector!");
        return;
      }

      _continueButton.OnContinueButtonClick += HandleContinueClicked;
    }

    private void OnDisable()
    {
      if (_continueButton == null)
        return;

      _continueButton.OnContinueButtonClick -= HandleContinueClicked;
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
  }
}