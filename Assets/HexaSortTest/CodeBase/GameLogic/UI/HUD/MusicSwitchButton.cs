using System;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public class MusicSwitchButton : ButtonBase
  {
    
    [SerializeField, BoxGroup("BUTTON SETUP")] private Image _image;
    [SerializeField, BoxGroup("BUTTON SETUP")] private Sprite _musicOnSprite;
    [SerializeField, BoxGroup("BUTTON SETUP")] private Sprite _musicOffSprite;

    private void Awake() => 
      _image.sprite = AudioFacade.Instance.IsFXEnabled ? _musicOnSprite : _musicOffSprite;

    protected override void ButtonClick()
    {
      AudioFacade.Instance.PlayClick();
      AudioFacade.Instance.SetMusicEnabled(!AudioFacade.Instance.IsMusicEnabled);
      _image.sprite = AudioFacade.Instance.IsFXEnabled ? _musicOnSprite : _musicOffSprite;
    }
  }
}