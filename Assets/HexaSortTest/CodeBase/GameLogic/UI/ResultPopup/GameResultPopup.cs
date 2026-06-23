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

    [SerializeField, BoxGroup("BUTTON")] private GameResultContinueButton _continueButton;

    [SerializeField, BoxGroup("ANIMATION")] private float _openDuration = 0.3f;

    public event Action OnContinueClicked;

    private void OnEnable() =>
      _continueButton.OnContinueButtonClick += HandleContinueClicked;

    private void OnDisable() =>
      _continueButton.OnContinueButtonClick -= HandleContinueClicked;

    public void ShowVictory()
    {
      _victoryView.SetActive(true);
      _defeatView.SetActive(false);
      Open();
    }

    public void ShowDefeat()
    {
      _victoryView.SetActive(false);
      _defeatView.SetActive(true);
      Open();
    }

    public override void Open()
    {
      base.Open();
      transform.localScale = Vector3.zero;
      transform.DOScale(Vector3.one, _openDuration).SetEase(Ease.OutBack);
    }

    public override void Close() =>
      base.Close();

    private void HandleContinueClicked() =>
      OnContinueClicked?.Invoke();
  }
}