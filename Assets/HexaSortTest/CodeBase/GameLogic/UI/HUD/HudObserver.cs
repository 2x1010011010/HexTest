using Sirenix.OdinInspector;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public class HudObserver : UIWindow
  {
    [SerializeField, BoxGroup("BOOSTERS BUTTONS")] private HammerButton _hammerButton;
    [SerializeField, BoxGroup("BOOSTERS BUTTONS")] private Image _hammerCounterImage;
    [SerializeField, BoxGroup("BOOSTERS BUTTONS")] private HandButton _handButton;
    [SerializeField, BoxGroup("BOOSTERS BUTTONS")] private Image _handCounterImage;
    [SerializeField, BoxGroup("BOOSTERS BUTTONS")] private TMP_Text _hammerBoosterCounter;
    [SerializeField, BoxGroup("BOOSTERS BUTTONS")] private TMP_Text _handBoostaerCounter;
    
    //[SerializeField, BoxGroup("COINS COUNTER")] private TMP_Text _coinsCounter;
    [SerializeField, BoxGroup("TILES COUNTER")] private TMP_Text _tilesCounter;
    [SerializeField, BoxGroup("TILES COUNTER")] private Slider _tilesCounterSlider;

    public static HudObserver Instance { get; private set; }

    private int _coisnsCount;
    private int _tilesCount;
    private int _hammerBoosterCount;
    private int _handBoosterCount;
    private int _tilesCounterSliderFill = 0;
    
    private void Awake()
    {
      if (Instance != null) Destroy(gameObject);
      Instance = this;
      
      _hammerBoosterCount = 2;
      _handBoosterCount = 2;
      _coisnsCount = 0;
      _tilesCount = 0;
      
      _hammerBoosterCounter.text = _hammerBoosterCount.ToString();
      _handBoostaerCounter.text = _handBoosterCount.ToString();
//      _coinsCounter.text = _coisnsCount.ToString();
      _tilesCounter.text = _tilesCount.ToString();
      _tilesCounterSlider.value = _tilesCount;
    }
    
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
      AudioFacade.Instance.PlayClick();
      if (_hammerBoosterCount <= 0) return;
      _hammerBoosterCount--;
      _hammerBoosterCounter.text = _hammerBoosterCount.ToString();
    }
    
    private void OnHandButtonClick()
    {
      AudioFacade.Instance.PlayClick();
      if (_handBoosterCount <= 0) return;
      _handBoosterCount--;
      _handBoostaerCounter.text = _handBoosterCount.ToString();
    }
    
    private void OnCoinsCounterChanged(int value)
    {
      
    }

    public void AddTiles(int value)
    {
      OnTilesCounterChanged(value);
    }

    private void OnTilesCounterChanged(int value)
    {
      _tilesCount += value;
      _tilesCounter.text = _tilesCount.ToString();
      _tilesCounterSliderFill += value;
      if (_tilesCounterSliderFill >= _tilesCounterSlider.maxValue)
      {
        _tilesCounterSliderFill = 0;
        GetRandomBooster();
      }
      
      _tilesCounterSlider.value = _tilesCounterSliderFill;
    }

    private void GetRandomBooster()
    {
      var randomBooster = Random.Range(0, 2);
      switch (randomBooster)
      {
        case 0:
          _hammerCounterImage.transform
            .DOPunchScale(Vector3.one * 2f, 0.5f, 10, 0.5f)
            .SetEase(Ease.OutBounce);
          _hammerBoosterCount++;
          _hammerBoosterCounter.text = _hammerBoosterCount.ToString();
          break;
        case 1:
          _handCounterImage.transform
            .DOPunchScale(Vector3.one * 2f, 0.5f, 10, 0.5f)
            .SetEase(Ease.OutBounce);
          _handBoosterCount++;
          _handBoostaerCounter.text = _handBoosterCount.ToString();
          break;
      }
    }
  }
}