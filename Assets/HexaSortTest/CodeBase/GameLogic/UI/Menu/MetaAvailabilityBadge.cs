using System.Collections;
using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.Infrastructure;
using HexaSortTest.CodeBase.Infrastructure.Services.CurrencyService;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace HexaSortTest.CodeBase.GameLogic.UI.Menu
{
  public sealed class MetaAvailabilityBadge : MonoBehaviour
  {
    [SerializeField] private MetaTileConfigsList _catalog;
    [SerializeField] private GameObject _badge;
    private ICurrencyService _currency;
    private IPersistentProgressService _progress;
    private Coroutine _refresh;

    [Inject]
    public void Construct(ICurrencyService currency, IPersistentProgressService progress)
    {
      _currency = currency;
      _progress = progress;
      Refresh();
    }
    private void OnEnable()
    {
      SceneManager.activeSceneChanged += HandleActiveSceneChanged;
      Refresh();
      _refresh = StartCoroutine(WatchProgress());
    }
    private void OnDisable()
    {
      SceneManager.activeSceneChanged -= HandleActiveSceneChanged;
      if (_refresh != null) StopCoroutine(_refresh);
      _refresh = null;
    }
    private void HandleActiveSceneChanged(Scene previous, Scene current) => Refresh();

    private IEnumerator WatchProgress()
    {
      // Purchases save progress after currency callbacks. Read the committed state,
      // also when this shared bottom panel is used outside the main menu.
      var delay = new WaitForSecondsRealtime(0.25f);
      while (true) { yield return delay; Refresh(); }
    }
    private void Refresh()
    {
      bool visible = false;
      // Opening Meta only hides the reminder; it does not acknowledge or spend it.
      // Re-evaluate affordability when returning to Home or Shop, even without a purchase.
      string scene = SceneManager.GetActiveScene().name;
      bool showInThisScene = scene == Constants.MainMenuScene || scene == Constants.ShopScene;
      if (showInThisScene && _currency != null && _progress?.PlayerProgress != null && _catalog?.Tiles != null)
      {
        int index = MetaCatalogProgress.FindInitialTile(_catalog, _progress.PlayerProgress.MetaProgress);
        var tile = index >= 0 ? _catalog.Tiles[index].TilePrefab : null;
        var saved = tile == null ? null : _progress.PlayerProgress.MetaProgress?.Find(p => p != null && p.TileId == tile.TileId);
        visible = tile != null && tile.TryGetCheapestClosedCellCost(saved, out int cost) && _currency.HexCoins >= cost;
      }
      if (_badge != null && _badge.activeSelf != visible) _badge.SetActive(visible);
    }
  }
}
