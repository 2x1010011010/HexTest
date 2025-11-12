using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public class MusicSwitchButton : ButtonBase
  {
    public event Action OnMusicSwitchButtonClick;
    
    [SerializeField, BoxGroup("BUTTON SETUP")] private Image _image;
    [SerializeField, BoxGroup("BUTTON SETUP")] private Sprite _musicOnSprite;
    [SerializeField, BoxGroup("BUTTON SETUP")] private Sprite _musicOffSprite;

    private void Awake()
    {
    }

    protected override void ButtonClick()
    {
    }
  }
}