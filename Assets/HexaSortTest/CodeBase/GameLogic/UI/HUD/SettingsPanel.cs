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
      base.Open();
      transform.DOMoveY(_openYPosition, _panelOpenDuration).SetEase(Ease.Linear);
    }
    
    public override void Close()
    {
      transform.DOMoveY(_closeYPosition, _panelOpenDuration)
        .SetEase(Ease.Linear)
        .OnComplete(() => base.Close());
    }
  }
}