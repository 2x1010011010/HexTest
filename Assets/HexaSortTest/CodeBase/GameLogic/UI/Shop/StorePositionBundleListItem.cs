using System;
using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.Infrastructure.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HexaSortTest.CodeBase.GameLogic.UI.Shop
{
  public class StorePositionBundleListItem : MonoBehaviour
  {
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _priceText;
    [SerializeField] private ShopPurchaseButton _purchaseButton;
    public StorePositionBundle Bundle { get; private set; }
    private Action<StorePositionBundle> _purchase;
    private void OnEnable() { if (_purchaseButton != null) _purchaseButton.Clicked += Buy; }
    private void OnDisable() { if (_purchaseButton != null) _purchaseButton.Clicked -= Buy; }
    private void Buy() => _purchase?.Invoke(Bundle);
    public void Setup(StorePositionBundle bundle) => Setup(bundle, null);
    public void Setup(StorePositionBundle bundle, Action<StorePositionBundle> purchase)
    {
      Bundle = bundle; _purchase = purchase;
      if (bundle == null) return;
      if (_icon != null) { _icon.sprite = bundle.Icon; _icon.enabled = bundle.Icon != null; }
      if (_nameText != null) _nameText.text = bundle.DisplayName;
      if (_descriptionText != null) _descriptionText.text = bundle.Description;
      Refresh(false, false);
    }
    public void Refresh(bool owned, bool allowTestPurchase)
    {
      bool valid = ShopService.Validate(Bundle, out _);
      if (_priceText != null) _priceText.text = owned ? "OWNED" :
        !valid ? "UNAVAILABLE" : allowTestPurchase ? "TEST" : "SOON";
      if (_purchaseButton != null) _purchaseButton.Button.interactable = valid && !owned && allowTestPurchase;
    }
  }
}
