using System;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public class SoundFxSwitchButton : ButtonBase
  {
    [SerializeField, BoxGroup("BUTTON SETUP")] private Image _image;
    [SerializeField, BoxGroup("BUTTON SETUP")] private Sprite _soundFxOnSprite;
    [SerializeField, BoxGroup("BUTTON SETUP")] private Sprite _soundFxOffSprite;

    private void Awake() =>
      _image.sprite = AudioFacade.Instance.IsFXEnabled ? _soundFxOnSprite : _soundFxOffSprite;

    protected override void ButtonClick()
    {
      AudioFacade.Instance.PlayClick();
      AudioFacade.Instance.SetFXEnabled(!AudioFacade.Instance.IsFXEnabled);
      _image.sprite = AudioFacade.Instance.IsFXEnabled ? _soundFxOnSprite : _soundFxOffSprite;
    }
  }
}