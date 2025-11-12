using Sirenix.OdinInspector;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public class HudObserver : UIWindow
  {
    [SerializeField, BoxGroup("BOOSTERS BUTTONS")] private HammerButton _hammerButton;
    [SerializeField, BoxGroup("BOOSTERS BUTTONS")] private HandButton _handButton;
    [SerializeField, BoxGroup("COINS COUNTER")] private TMP_Text _coinsCounter;
    [SerializeField, BoxGroup("TILES COUNTER")] private TMP_Text _tilesCounter;
    [SerializeField, BoxGroup("TILES COUNTER")] private Slider _tilesCounterSlider;
    
    private void OnEnable()
    {
      _hammerButton.OnHammerButtonClick += OnHammerButtonClick;
      _handButton.OnHandButtonClick += OnHandButtonClick;
      
      Open();
    }

    private void OnDisable()
    {
      Close();
    }

    private void OnHammerButtonClick()
    {
    }
    
    private void OnHandButtonClick()
    {
    }
    
    private void OnCoinsCounterChanged(int value)
    {
    }
    
    private void OnTilesCounterChanged(int value)
    {
    }
  }
}