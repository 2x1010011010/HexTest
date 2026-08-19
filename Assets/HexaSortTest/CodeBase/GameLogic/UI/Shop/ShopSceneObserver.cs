using System;
using System.Collections.Generic;
using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.Infrastructure.Services.AssetManagement;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.Shop
{
  public class ShopSceneObserver : MonoBehaviour
  {
    [SerializeField, BoxGroup("BUTTONS")] private ShopExitButton _exitButton;

    [SerializeField, BoxGroup("LIST SETUP")] private Transform _bundlesContainer;
    [SerializeField, BoxGroup("LIST SETUP")] private StorePositionBundleListItem _bundleItemPrefab;

    [SerializeField, BoxGroup("STATUS"), Tooltip("Shown when there's nothing to list (missing catalog or empty). Hidden once the list is populated.")]
    private TMP_Text _statusText;

    public event Action OnExitRequested;

    private readonly List<StorePositionBundleListItem> _listItems = new();

    private void Start() =>
      BuildBundleList();

    private void OnEnable()
    {
      if (_exitButton != null)
        _exitButton.OnExitButtonClick += HandleExitClicked;
      else
        Debug.LogError("[ShopSceneObserver] _exitButton is not assigned in the inspector!");
    }

    private void OnDisable()
    {
      if (_exitButton != null)
        _exitButton.OnExitButtonClick -= HandleExitClicked;
    }

    private void BuildBundleList()
    {
      ClearBundleList();

      if (_bundleItemPrefab == null || _bundlesContainer == null)
      {
        Debug.LogError("[ShopSceneObserver] _bundleItemPrefab or _bundlesContainer is not assigned!");
        return;
      }

      var catalog = Resources.Load<StorePositionBundleCatalog>(AssetPaths.StorePositionBundleCatalog);

      if (catalog == null)
      {
        Debug.LogError($"[ShopSceneObserver] No StorePositionBundleCatalog found at " +
                        $"Resources/{AssetPaths.StorePositionBundleCatalog}.");
        SetStatus("No products available.");
        return;
      }

      if (catalog.Bundles == null || catalog.Bundles.Count == 0)
      {
        SetStatus("No products available.");
        return;
      }

      SetStatus(null);

      foreach (var bundle in catalog.Bundles)
      {
        if (bundle == null)
          continue;

        var item = Instantiate(_bundleItemPrefab, _bundlesContainer);
        item.Setup(bundle);
        _listItems.Add(item);
      }
    }

    private void ClearBundleList()
    {
      foreach (var item in _listItems)
        if (item != null)
          Destroy(item.gameObject);

      _listItems.Clear();
    }

    private void SetStatus(string message)
    {
      if (_statusText == null)
        return;

      _statusText.gameObject.SetActive(!string.IsNullOrEmpty(message));
      _statusText.text = message;
    }

    private void HandleExitClicked() =>
      OnExitRequested?.Invoke();
  }
}
