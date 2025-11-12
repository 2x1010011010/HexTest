using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public class SoundFxSwitchButton : ButtonBase
  {
    public event Action OnSoundFxSwitchButtonClick;
    
    [SerializeField, BoxGroup("BUTTON SETUP")] private Image _image;
    [SerializeField, BoxGroup("BUTTON SETUP")] private Sprite _soundFxOnSprite;
    [SerializeField, BoxGroup("BUTTON SETUP")] private Sprite _soundFxOffSprite;

    private void Awake()
    {
    }

    protected override void ButtonClick()
    {
      OnSoundFxSwitchButtonClick?.Invoke();
    }
  }
}