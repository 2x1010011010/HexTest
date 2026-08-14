using Sirenix.OdinInspector;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using HexaSortTest.CodeBase.GameLogic.Boosters;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;
using HexaSortTest.CodeBase.GameLogic.Spawners;
using HexaSortTest.CodeBase.GameLogic.UI.MainMenu;
using HexaSortTest.CodeBase.Infrastructure.Services.BoosterInventoryService;
using HexaSortTest.CodeBase.Infrastructure.Services.CurrencyService;
using Zenject;
using Random = UnityEngine.Random;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public class HudObserver : UIWindow
  {
    [SerializeField, BoxGroup("BOOSTERS TOOLS SETUP")] private BoosterTools _boosterTools;
    
    [SerializeField, BoxGroup("BOOSTERS BUTTONS")] private HammerButton _hammerButton;
    [SerializeField, BoxGroup("BOOSTERS BUTTONS")] private Image _hammerCounterImage;
    [SerializeField, BoxGroup("BOOSTERS BUTTONS")] private HandButton _handButton;
    [SerializeField, BoxGroup("BOOSTERS BUTTONS")] private Image _handCounterImage;
    [SerializeField, BoxGroup("BOOSTERS BUTTONS")] private RespawnButton _respawnButton;
    [SerializeField, BoxGroup("BOOSTERS BUTTONS")] private Image _respawnImage;
    [SerializeField, BoxGroup("BOOSTERS BUTTONS")] private TMP_Text _hammerBoosterCounter;
    [SerializeField, BoxGroup("BOOSTERS BUTTONS")] private TMP_Text _handBoosterCounter;
    [SerializeField, BoxGroup("BOOSTERS BUTTONS")] private TMP_Text _respawnBoosterCounter;

    [SerializeField, BoxGroup("COINS COUNTER")] private TMP_Text _coinsCounter;
    [SerializeField, BoxGroup("TILES COUNTER")] private TMP_Text _tilesCounter;
    [SerializeField, BoxGroup("TILES COUNTER")] private TMP_Text _winConditionText;
    [SerializeField, BoxGroup("TILES COUNTER")] private Slider _tilesCounterSlider;

    [Inject] private ICurrencyService _currencyService;
    [Inject] private IBoosterInventoryService _boosterInventory;

    public static HudObserver Instance { get; private set; }

    private UIWindow _mainMenu;
    private StacksSpawner _stacksSpawner;
    private GridObserver _gridObserver;
    private BoosterPurchasePopup _boosterPurchasePopup;
    private int _winCondition;
    private int _tilesCount;
    private int _tilesCounterSliderFill = 0;

    private void Awake()
    {
      if (Instance != null) Destroy(gameObject);
      Instance = this;

      _tilesCount = 0;
      _tilesCounter.text = _tilesCount.ToString();
      _tilesCounterSlider.value = _tilesCount;
    }

    public void Init(int configWinCondition, MainMenuObserver mainMenu, StacksSpawner stacksSpawner, GridObserver gridObserver = null, BoosterPurchasePopup boosterPurchasePopup = null)
    {
      _winCondition = configWinCondition;
      _mainMenu = mainMenu;
      _stacksSpawner = stacksSpawner;
      _gridObserver = gridObserver;
      _boosterPurchasePopup = boosterPurchasePopup;
      _boosterTools.SetSpawner(_stacksSpawner);
    }

    private void OnEnable()
    {
      _hammerButton.OnHammerButtonClick += OnHammerButtonClick;
      _handButton.OnHandButtonClick += OnHandButtonClick;
      _respawnButton.OnRespawnButtonClick += OnRespawnButtonClick;

      if (_currencyService != null)
        _currencyService.OnCoinsChanged += HandleCoinsChanged;

      if (_boosterInventory != null)
        _boosterInventory.OnCountChanged += HandleBoosterCountChanged;

      Open();
    }

    private void Start()
    {
      _tilesCounterSlider.maxValue = _winCondition;
      _tilesCounterSliderFill = 0;
      _winConditionText.text = ("/" +_winCondition).ToString();
      _tilesCounter.text = _tilesCount.ToString();

      RefreshCoinsCounter();
      RefreshAllBoosterCounters();
    }

    private void OnDisable()
    {
      Close();
      _hammerButton.OnHammerButtonClick -= OnHammerButtonClick;
      _handButton.OnHandButtonClick -= OnHandButtonClick;
      _respawnButton.OnRespawnButtonClick -= OnRespawnButtonClick;

      if (_currencyService != null)
        _currencyService.OnCoinsChanged -= HandleCoinsChanged;

      if (_boosterInventory != null)
        _boosterInventory.OnCountChanged -= HandleBoosterCountChanged;
    }

    private void OnRespawnButtonClick(IBooster booster) => TryUseOrPromptPurchase(BoosterType.Respawn, booster);
    private void OnHammerButtonClick(IBooster booster) => TryUseOrPromptPurchase(BoosterType.Hammer, booster);
    private void OnHandButtonClick(IBooster booster) => TryUseOrPromptPurchase(BoosterType.Hand, booster);

    private void TryUseOrPromptPurchase(BoosterType type, IBooster booster)
    {
      AudioFacade.Instance.PlayClick();

      if (_boosterInventory == null)
      {
        Debug.LogError("[HudObserver] IBoosterInventoryService not injected.");
        return;
      }

      if (_boosterInventory.TrySpend(type))
      {
        _boosterTools.ActivateBooster(booster);
        return;
      }

      if (_boosterPurchasePopup != null)
        _boosterPurchasePopup.ShowFor(type);
      else
        Debug.LogWarning("[HudObserver] BoosterPurchasePopup is not set, cannot prompt for purchase.");
    }

    private void HandleBoosterCountChanged(BoosterType type, int newCount)
    {
      switch (type)
      {
        case BoosterType.Hammer:
          _hammerBoosterCounter.text = newCount.ToString();
          break;
        case BoosterType.Hand:
          _handBoosterCounter.text = newCount.ToString();
          break;
        case BoosterType.Respawn:
          _respawnBoosterCounter.text = newCount.ToString();
          break;
      }
    }

    private void RefreshAllBoosterCounters()
    {
      if (_boosterInventory == null) return;

      _hammerBoosterCounter.text = _boosterInventory.GetCount(BoosterType.Hammer).ToString();
      _handBoosterCounter.text = _boosterInventory.GetCount(BoosterType.Hand).ToString();
      _respawnBoosterCounter.text = _boosterInventory.GetCount(BoosterType.Respawn).ToString();
    }

    private void HandleCoinsChanged(int value)
    {
      if (_coinsCounter != null)
        _coinsCounter.text = value.ToString();
    }

    private void RefreshCoinsCounter()
    {
      if (_coinsCounter != null && _currencyService != null)
        _coinsCounter.text = _currencyService.Coins.ToString();
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

      Debug.Log($"[HudObserver] tilesCounterSliderFill={_tilesCounterSliderFill}, maxValue={_tilesCounterSlider.maxValue}, gridObserverSet={(_gridObserver != null)}");

      if (_tilesCounterSliderFill >= _tilesCounterSlider.maxValue)
      {
        _tilesCounterSliderFill = 0;

        Debug.Log("[HudObserver] Win condition reached, triggering victory.");

        if (_gridObserver != null)
          _gridObserver.TriggerVictory();
        else if (_mainMenu != null)
          _mainMenu.Open();
        else
          Debug.LogError("[HudObserver] Win condition reached but neither GridObserver nor MainMenu is set!");
      }

      _tilesCounterSlider.value = _tilesCounterSliderFill;
    }

    // Pre-existing, unused reward-roll helper (dead code before this change
    // too — nothing calls it). Repointed at IBoosterInventoryService since
    // the local booster-count fields it used to mutate no longer exist.
    private void GetRandomBooster()
    {
      if (_boosterInventory == null) return;

      var randomBooster = Random.Range(0, 32);
      switch (randomBooster)
      {
        case 0:
        case 5:
        case 12:
        case 23:
        case 29:
          _hammerCounterImage.transform
            .DOPunchScale(Vector3.one * 2f, 0.5f, 10, 0.5f)
            .SetEase(Ease.OutBounce);
          _boosterInventory.Add(BoosterType.Hammer, 1);
          break;
        
        case 1:
        case 8:
        case 15:
        case 24:
        case 26:
          _handCounterImage.transform
            .DOPunchScale(Vector3.one * 2f, 0.5f, 10, 0.5f)
            .SetEase(Ease.OutBounce);
          _boosterInventory.Add(BoosterType.Hand, 1);
          break;
      }
    }
  }
}
