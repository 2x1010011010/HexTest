using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.Boosters;
using HexaSortTest.CodeBase.Infrastructure.Services.AssetManagement;
using HexaSortTest.CodeBase.Infrastructure.Services.BoosterInventoryService;
using HexaSortTest.CodeBase.Infrastructure.Services.CurrencyService;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace HexaSortTest.CodeBase.GameLogic.UI.HUD
{
  public class BoosterPurchasePopup : UIWindow
  {
    [SerializeField, BoxGroup("SETUP")] private TMP_Text _titleText;
    [SerializeField, BoxGroup("SETUP")] private TMP_Text _priceText;
    [SerializeField, BoxGroup("SETUP")] private Image _icon;

    [SerializeField, BoxGroup("BUTTONS")] private BoosterBuyButton _buyButton;
    [SerializeField, BoxGroup("BUTTONS")] private BoosterWatchAdButton _watchAdButton;
    [SerializeField, BoxGroup("BUTTONS")] private PopupCloseButton _closeButton;

    [Inject] private ICurrencyService _currencyService;
    [Inject] private IBoosterInventoryService _boosterInventory;

    private BoosterShopConfig _shopConfig;
    private BoosterPriceEntry _pendingEntry;
    private BoosterType _pendingType;

    private void Awake() =>
      _shopConfig = Resources.Load<BoosterShopConfig>(AssetPaths.BoosterShopConfig);

    private void OnEnable()
    {
      if (_buyButton != null)
        _buyButton.OnBuyClicked += HandleBuyClicked;
      else
        Debug.LogError("[BoosterPurchasePopup] _buyButton is not assigned in the inspector!");

      if (_watchAdButton != null)
        _watchAdButton.OnWatchAdClicked += HandleWatchAdClicked;
      else
        Debug.LogError("[BoosterPurchasePopup] _watchAdButton is not assigned in the inspector!");

      if (_closeButton != null)
        _closeButton.OnCloseClicked += Close;
      else
        Debug.LogError("[BoosterPurchasePopup] _closeButton is not assigned in the inspector!");
    }

    private void OnDisable()
    {
      if (_buyButton != null) _buyButton.OnBuyClicked -= HandleBuyClicked;
      if (_watchAdButton != null) _watchAdButton.OnWatchAdClicked -= HandleWatchAdClicked;
      if (_closeButton != null) _closeButton.OnCloseClicked -= Close;
    }

    public void ShowFor(BoosterType boosterType)
    {
      _pendingType = boosterType;
      _pendingEntry = FindEntry(boosterType);

      if (_pendingEntry == null)
      {
        Debug.LogError($"[BoosterPurchasePopup] No BoosterShopConfig entry found for {boosterType}. " +
                        "Add one in Resources/StaticData/GameConfigs/BoosterShopConfig.");
        return;
      }

      if (_titleText != null) _titleText.text = _pendingEntry.DisplayName;
      if (_priceText != null) _priceText.text = _pendingEntry.PriceInCoins.ToString();
      if (_icon != null && _pendingEntry.Icon != null) _icon.sprite = _pendingEntry.Icon;

      Open();
    }

    private BoosterPriceEntry FindEntry(BoosterType type) =>
      _shopConfig != null && _shopConfig.Prices != null
        ? _shopConfig.Prices.Find(p => p.BoosterType == type)
        : null;

    private void HandleBuyClicked()
    {
      if (_pendingEntry == null)
        return;

      if (!_currencyService.TrySpendCoins(_pendingEntry.PriceInCoins))
      {
        Debug.Log("[BoosterPurchasePopup] Not enough Coins.");
        return;
      }
      _boosterInventory.Add(_pendingType, 1);
      Close();
    }

    private void HandleWatchAdClicked()
    {
      if (_pendingEntry == null)
        return;

      Debug.Log($"[BoosterPurchasePopup] (stub) Reward ad 'watched' for {_pendingType} — granting 1 for free.");
      _boosterInventory.Add(_pendingType, 1);
      Close();
    }
  }
}
