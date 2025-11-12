using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public class SettingsPanel : UIPanel
  {
    [SerializeField, BoxGroup("SETTINGS PANEL SETUP")] private float _panelOpenDuration = 0.5f;
    [SerializeField, BoxGroup("SETTINGS PANEL SETUP")] private float _closeYPosition;
    [SerializeField, BoxGroup("SETTINGS PANEL SETUP")] private float _openYPosition;
    [SerializeField, BoxGroup("SETTINGS PANEL SETUP")] private SettingsButton _settingsButton;

    private void Awake() => Close();
    
    private void OnEnable()
    {
      _settingsButton.OnSettingsButtonClick += Action;
    }

    private void OnDisable()
    {
      _settingsButton.OnSettingsButtonClick += Action;
    }

    private void Action()
    {
      if (IsOpen) Close();
      else Open();
    }

    public override void Open()
    {
      if (IsOpen) return;
      gameObject.SetActive(true);
      transform.DOLocalMoveY(_openYPosition, _panelOpenDuration)
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
          IsOpen = true;
        });
    }
    
    public override void Close()
    {
      if (!IsOpen) return;
      transform.DOLocalMoveY(_closeYPosition, _panelOpenDuration)
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
          IsOpen = false;
          gameObject.SetActive(false);
        });
    }
  }
}