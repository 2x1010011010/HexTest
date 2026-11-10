using System.Collections.Generic;
using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.Infrastructure.Services;
using HexaSortTest.CodeBase.Infrastructure.Services.AssetManagement;
using HexaSortTest.CodeBase.Infrastructure.Services.CurrencyService;
using LifeService = HexaSortTest.CodeBase.Infrastructure.Services.LivesService.LivesService;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Zenject;

namespace HexaSortTest.CodeBase.GameLogic.UI.Shop
{
  public class ShopSceneObserver : MonoBehaviour
  {
    [SerializeField, BoxGroup("LIST SETUP")] private Transform _bundlesContainer;
    [SerializeField, BoxGroup("LIST SETUP")] private StorePositionBundleListItem _bundleItemPrefab;
    [SerializeField] private StorePositionBundleCatalog _catalog;
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private TMP_Text _balanceText;

    [SerializeField, Tooltip("Only works in Editor or Development Build. No real payments.")]
    private bool _enableTestPurchases = true;

    private readonly List<StorePositionBundleListItem> _listItems = new();
    private ShopService _shop;
    private ICurrencyService _currency;
    private LifeService _lives;
    private float _nextLivesRefresh;
    private bool CanTest => _enableTestPurchases && (Application.isEditor || Debug.isDebugBuild);

    [Inject]
    public void Construct(ShopService shop, ICurrencyService currency, LifeService lives)
    {
      _shop = shop;
      _currency = currency;
      _lives = lives;
    }

    private void Start()
    {
      if (_shop == null)
      {
        SetStatus("Start from the Bootstrap scene.");
        return;
      }

      _shop.Changed += Refresh;
      _currency.OnCoinsChanged += CoinsChanged;
      _lives.OnLivesChanged += CoinsChanged;
      BuildBundleList();
      Refresh();
    }

    private void Update()
    {
      if (_lives == null || Time.unscaledTime < _nextLivesRefresh) return;
      _nextLivesRefresh = Time.unscaledTime + 1f;
      _lives.Refresh();
    }

    private void OnDestroy()
    {
      if (_shop != null) _shop.Changed -= Refresh;
      if (_currency != null) _currency.OnCoinsChanged -= CoinsChanged;
      if (_lives != null) _lives.OnLivesChanged -= CoinsChanged;
    }

    private void CoinsChanged(int _) => Refresh();

    private void BuildBundleList()
    {
      foreach (var item in _listItems)
        if (item != null)
          Destroy(item.gameObject);
      _listItems.Clear();
      if (_bundlesContainer != null)
        foreach (Transform child in _bundlesContainer)
        {
          child.gameObject.SetActive(false);
          Destroy(child.gameObject);
        }

      var catalog = _catalog != null
        ? _catalog
        : Resources.Load<StorePositionBundleCatalog>(AssetPaths.StorePositionBundleCatalog);
      if (_bundleItemPrefab == null || _bundlesContainer == null || catalog?.Bundles == null)
      {
        SetStatus("No products available.");
        return;
      }

      var ids = new HashSet<string>();
      foreach (var bundle in catalog.Bundles)
      {
        if (!ShopService.Validate(bundle, out _) || !ids.Add(bundle.Sku)) continue;
        var item = Instantiate(_bundleItemPrefab, _bundlesContainer);
        item.Setup(bundle, Purchase);
        _listItems.Add(item);
      }

      SetStatus(_listItems.Count == 0 ? "No products available." :
        CanTest ? "Test purchases - no real payments" : "Store coming soon");
    }

    private void Purchase(StorePositionBundle product)
    {
      _shop.TryPurchaseStub(product, CanTest, out string message);
      SetStatus(message);
      Refresh();
    }

    private void Refresh()
    {
      if (_balanceText != null)
        _balanceText.text = $"COINS  {_currency.Coins:N0}     LIVES  {_lives.Lives}/{_lives.MaxLives}";
      foreach (var item in _listItems)
        if (item != null)
          item.Refresh(_shop.IsOwned(item.Bundle), CanTest);
    }

    private void SetStatus(string message)
    {
      if (_statusText != null)
      {
        _statusText.gameObject.SetActive(true);
        _statusText.text = message;
      }
    }
  }
}