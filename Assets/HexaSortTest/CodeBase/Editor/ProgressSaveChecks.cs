using HexaSortTest.CodeBase.Infrastructure.Services.LivesService;
using HexaSortTest.CodeBase.Infrastructure.StateMachine.States.CustomPayloadStructures;
using System;
using System.Collections.Generic;
using System.Reflection;
using HexaSortTest.CodeBase.GameConfigs;
using HexaSortTest.CodeBase.GameLogic.Boosters;
using HexaSortTest.CodeBase.GameLogic.Data;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using HexaSortTest.CodeBase.GameLogic.Meta;
using HexaSortTest.CodeBase.GameLogic.Spawners;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using HexaSortTest.CodeBase.GameLogic.UI.MainMenu;
using HexaSortTest.CodeBase.Infrastructure;
using HexaSortTest.CodeBase.Infrastructure.Services.BoosterInventoryService;
using HexaSortTest.CodeBase.Infrastructure.Services.CurrencyService;
using HexaSortTest.CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.Infrastructure.Services.MetaProgressService;
using HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using HexaSortTest.CodeBase.Infrastructure.Services.SaveAndLoadService;
using HexaSortTest.CodeBase.Infrastructure.StateMachine.States;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HexaSortTest.CodeBase.Editor
{
  // Uses the real Unity JsonUtility/PlayerPrefs and runtime services, with isolated keys.
  // No assembly layout changes or NUnit dependency are needed by the game scripts.
  public static class ProgressSaveChecks
  {
    [MenuItem("Tools/Tests/Run Progress Save Checks")]
    public static void RunAll()
    {
      if (EditorApplication.isPlayingOrWillChangePlaymode)
        throw new InvalidOperationException("Run progress checks outside Play Mode.");

      int count = 0;
      Run(RoundTrip, ref count);
      Run(LegacyMigration, ref count);
      Run(CorruptMainRecoversBackup, ref count);
      Run(InvalidSavesReturnNull, ref count);
      Run(TransactionKeepsOneRecoveryPoint, ref count);
      Run(TransactionRollsBack, ref count);
      Run(PurchasePersistsBothHalves, ref count);
      Run(PartialMetaAndCompletion, ref count);
      Run(InsufficientFundsDoNotAdvanceMeta, ref count);
      Run(IndividualMetaCellPrices, ref count);
      Run(SelectedCellPurchaseAndReload, ref count);
      Run(LegacyCellMigrationAndReorder, ref count);
      Run(SelectedCellTransactionRollback, ref count);
      Run(HexHitGeometry, ref count);
      Run(VictoryIsCommittedOnce, ref count);
      Run(DefeatRewardsAndLives, ref count);
      Run(AbandonAttemptSpendsExactlyOnce, ref count);
      Run(NewerSaveIsNotOverwritten, ref count);
      Debug.Log($"[ProgressSaveChecks] PASS: {count} checks. Player profile was not touched.");
    }

    private static void Run(Action<Fixture> check, ref int count)
    {
      using (var fixture = new Fixture()) check(fixture);
      count++;
    }

    private static void RoundTrip(Fixture f)
    {
      f.Progress.LevelIndex = 8;
      f.Progress.Coins = 47;
      f.Progress.HexCoins = 130;
      f.Progress.Boosters[0].Count = 3;
      f.Progress.MetaProgress.Add(new MetaTileProgress("one") { CurrentObjectProgress = 0.4f });
      f.Save.SaveProgress();
      var loaded = f.Reload();
      Require(loaded.LevelIndex == 8 && loaded.Coins == 47 && loaded.HexCoins == 130, "Balances/level round trip");
      Require(loaded.Boosters[0].Count == 3 && Mathf.Approximately(loaded.MetaProgress[0].CurrentObjectProgress, 0.4f), "Inventory/meta round trip");
      Require(loaded.SaveVersion == PlayerProgress.CurrentSaveVersion, "Save version");
    }

    private static void LegacyMigration(Fixture f)
    {
      PlayerPrefs.SetString(f.Key, "{\"WorldData\":{\"LastLevel\":{\"Level\":\"Game\"}},\"LevelIndex\":4,\"Coins\":19,\"HexCoins\":-5}");
      var loaded = f.Reload();
      Require(loaded.LevelIndex == 4 && loaded.Coins == 19 && loaded.HexCoins == 0, "Legacy balances retained");
      Require(loaded.Boosters != null && loaded.MetaProgress != null, "Missing legacy lists initialized");
      Require(loaded.SaveVersion == PlayerProgress.CurrentSaveVersion, "Legacy version migrated");
    }

    private static void CorruptMainRecoversBackup(Fixture f)
    {
      f.Progress.Coins = 10;
      f.Save.SaveProgress();
      f.Progress.Coins = 20;
      f.Save.SaveProgress();
      f.Save.SaveProgress(); // Duplicate lifecycle saves must retain the earlier snapshot.
      PlayerPrefs.SetString(f.Key, "not-json");
      var recovered = f.Reload();
      Require(recovered.Coins == 10, "Previous valid snapshot recovered");
      f.Service.PlayerProgress = recovered;
      f.Save.SaveProgress();
      Require(f.Reload().Coins == 10, "Recovered profile repaired main save");
    }

    private static void InvalidSavesReturnNull(Fixture f)
    {
      Require(f.Reload() == null, "No save returns null");
      PlayerPrefs.SetString(f.Key, "{}");
      PlayerPrefs.SetString(f.Key + ".Backup", "broken-json");
      Require(f.Reload() == null, "Unrelated/corrupt JSON rejected");
    }

    private static void TransactionKeepsOneRecoveryPoint(Fixture f)
    {
      f.Save.SaveProgress();
      f.Save.ExecuteTransaction(() =>
      {
        f.Currency.AddCoins(5);
        Require(f.Reload().Coins == 0, "Intermediate currency change must not be saved");
        f.Save.ExecuteTransaction(() => f.Currency.AddHexCoins(20));
        Require(f.Reload().HexCoins == 0, "Nested transaction must not commit early");
      });
      Require(f.Reload().Coins == 5 && f.Reload().HexCoins == 20, "Transaction commits both rewards");
      PlayerPrefs.SetString(f.Key, "broken-json");
      var backup = f.Reload();
      Require(backup.Coins == 0 && backup.HexCoins == 0, "Backup contains complete prior state");
    }

    private static void TransactionRollsBack(Fixture f)
    {
      f.Save.SaveProgress();
      var original = f.Progress;
      try
      {
        f.Save.ExecuteTransaction(() =>
        {
          f.Currency.AddCoins(17);
          throw new InvalidOperationException("Expected check failure");
        });
        throw new Exception("Transaction did not propagate failure");
      }
      catch (InvalidOperationException) { }
      Require(ReferenceEquals(original, f.Progress) && f.Progress.Coins == 0, "In-memory rollback preserves root identity");
      Require(f.Reload().Coins == 0, "Failed transaction leaves stored data unchanged");
      f.Currency.AddCoins(1);
      Require(f.Reload().Coins == 1, "Save batching resets after failure");
    }

    private static void PurchasePersistsBothHalves(Fixture f)
    {
      f.Currency.AddCoins(100);
      var inventory = new BoosterInventoryService(f.Service, f.Save);
      int before = inventory.GetCount(BoosterType.Hammer);
      f.Save.ExecuteTransaction(() =>
      {
        Require(f.Currency.TrySpendCoins(30), "Purchase funded");
        inventory.Add(BoosterType.Hammer, 1);
      });
      var loaded = f.Reload();
      Require(loaded.Coins == 70 && loaded.Boosters.Find(b => b.Type == BoosterType.Hammer).Count == before + 1,
        "Purchase persists debit and booster together");
    }

    private static void PartialMetaAndCompletion(Fixture f)
    {
      var tile = f.CreateTile();
      var observer = new MetaObserver(f.Service, f.Save, f.Currency);
      f.Currency.AddHexCoins(100);
      Require(observer.TryProgressTile(tile), "First meta step succeeds");
      var loaded = f.Reload();
      Require(loaded.HexCoins == 90 && Mathf.Approximately(loaded.MetaProgress[0].CurrentObjectProgress, 0.25f),
        "Partial paid step persisted");
      tile.InitializeFromSave(loaded.MetaProgress[0]);
      Require(Mathf.Approximately(tile.CurrentObjectProgress, 0.25f), "Partial visual state restored");
      bool sawCommittedCompletion = false;
      tile.OnTileFullyOpened += _ =>
      {
        var saved = f.Reload();
        sawCommittedCompletion = saved.MetaProgress[0].UnlockedGroupsCount == 1 && saved.HexCoins == 60;
      };
      observer.TryProgressTile(tile);
      observer.TryProgressTile(tile);
      observer.TryProgressTile(tile);
      Require(sawCommittedCompletion, "Completion event sees committed save");
      Require(!observer.TryProgressTile(tile) && f.Progress.HexCoins == 60, "Completed tile cannot charge again");
    }

    private static void InsufficientFundsDoNotAdvanceMeta(Fixture f)
    {
      var tile = f.CreateTile();
      var observer = new MetaObserver(f.Service, f.Save, f.Currency);
      Require(!observer.TryProgressTile(tile), "Unfunded step rejected");
      Require(tile.CurrentObjectProgress == 0f && f.Progress.MetaProgress.Count == 0, "No unpaid meta progress");
    }

    private static void IndividualMetaCellPrices(Fixture f)
    {
      var tile = f.CreateTile();
      var covers = new List<GameObject>();
      var frames = new List<GameObject>();
      foreach (int price in new[] { 7, 31, 0 })
      {
        var cellRoot = new GameObject("Cell");
        cellRoot.transform.SetParent(tile.transform, false);
        var cell = cellRoot.AddComponent<MetaLocationCell>();
        var cover = new GameObject("Cover");
        cover.transform.SetParent(cellRoot.transform, false);
        var frame = new GameObject("Frame");
        frame.transform.SetParent(cellRoot.transform, false);
        cell.Configure(price, cover, null, null, null, null, null);
        covers.Add(cover);
        frames.Add(frame);
      }
      tile.ConfigureAuthoring("priced-tile", 99, covers);
      var observer = new MetaObserver(f.Service, f.Save, f.Currency);
      f.Currency.AddHexCoins(7);
      Require(tile.HexCoinsCostPerStep == 7, "First cell has individual price");
      Require(observer.TryProgressTile(tile) && f.Currency.HexCoins == 0, "First cell charges 7, not next cell's 31");
      Require(!covers[0].activeSelf && frames[0].activeInHierarchy, "Opening preserves frame");
      Require(tile.HexCoinsCostPerStep == 31 && !observer.TryProgressTile(tile), "Unaffordable second cell stays closed");
      var saved = f.Reload();
      tile.InitializeFromSave(saved.MetaProgress.Find(p => p.TileId == tile.TileId));
      Require(!covers[0].activeSelf && covers[1].activeSelf && tile.HexCoinsCostPerStep == 31, "Reload restores price cursor");
      f.Currency.AddHexCoins(31);
      Require(observer.TryProgressTile(tile) && f.Currency.HexCoins == 0, "Second cell charges 31 even though next is free");
      Require(observer.TryProgressTile(tile) && tile.IsTileFullyOpen && f.Currency.HexCoins == 0, "Free final cell completes without charging fallback price");
      Require(!observer.TryProgressTile(tile), "No payment after completion");
      saved = f.Reload();
      Require(saved.HexCoins == 0 && saved.MetaProgress.Find(p => p.TileId == tile.TileId).UnlockedGroupsCount == 1,
        "Individual prices and final progress persist together");
    }

    private static List<MetaLocationCell> CreateCells(Fixture f, out MetaTile tile)
    {
      tile = f.CreateTile();
      var cells = new List<MetaLocationCell>();
      var covers = new List<GameObject>();
      foreach (int price in new[] { 7, 31, 0 })
      {
        var root = new GameObject("Cell");
        root.transform.SetParent(tile.transform, false);
        var cell = root.AddComponent<MetaLocationCell>();
        var cover = new GameObject("Cover");
        cover.transform.SetParent(root.transform, false);
        cell.Configure(price, cover, null, null, null, null, null);
        cells.Add(cell);
        covers.Add(cover);
      }
      tile.ConfigureAuthoring("selected-cell-test", 99, covers);
      return cells;
    }

    private static void SelectedCellPurchaseAndReload(Fixture f)
    {
      var cells = CreateCells(f, out var tile);
      var observer = new MetaObserver(f.Service, f.Save, f.Currency);
      f.Currency.AddHexCoins(31);
      Require(observer.TryOpenCell(tile, cells[1]) == MetaCellPurchaseResult.Success, "Select second cell directly");
      Require(f.Currency.HexCoins == 0 && tile.IsCellOpen(cells[1]) && !tile.IsCellOpen(cells[0]), "Only selected price/cell committed");
      Require(cells[1].CoverRoot.activeSelf, "Paid cover remains visible until presentation");
      Require(observer.TryOpenCell(tile, cells[1]) == MetaCellPurchaseResult.AlreadyOpened, "Repeated click cannot charge again");
      Require(observer.TryOpenCell(tile, cells[0]) == MetaCellPurchaseResult.InsufficientFunds, "Unaffordable selection rejected");
      Require(!tile.IsCellOpen(cells[0]) && f.Currency.HexCoins == 0, "Rejection leaves balance/progress intact");
      var saved = f.Reload().MetaProgress.Find(p => p.TileId == tile.TileId);
      Require(saved.UsesCellIds && saved.OpenedCellIds.Count == 1 && saved.OpenedCellIds[0] == cells[1].CellId,
        "Selected ID survives JSON round trip");
      tile.InitializeFromSave(saved);
      Require(!cells[1].CoverRoot.activeSelf && cells[0].CoverRoot.activeSelf, "Reload during animation reveals paid cell");
      cells[1].PlayReveal(); // Null lock and null ParticleSystem must be safe.
      Require(observer.TryOpenCell(tile, cells[2]) == MetaCellPurchaseResult.Success && !tile.IsTileFullyOpen, "Free cell opens once");
      f.Currency.AddHexCoins(7);
      int completed = 0;
      tile.OnTileFullyOpened += _ =>
      {
        completed++;
        Require(f.Reload().MetaProgress[0].OpenedCellIds.Count == 3, "Completion observes committed IDs");
      };
      Require(observer.TryOpenCell(tile, cells[0]) == MetaCellPurchaseResult.Success && tile.IsTileFullyOpen, "Last missing cell completes tile");
      Require(observer.TryOpenCell(tile, cells[0]) == MetaCellPurchaseResult.AlreadyOpened && completed == 1, "No duplicate completion/payment");
      var foreign = CreateCells(f, out var otherTile);
      Require(observer.TryOpenCell(tile, foreign[0]) == MetaCellPurchaseResult.Invalid, "Foreign cell rejected even with same tile name");
    }

    private static void LegacyCellMigrationAndReorder(Fixture f)
    {
      var cells = CreateCells(f, out var tile);
      tile.InitializeFromSave(new MetaTileProgress(tile.TileId) { UnlockedObjectsInGroupCount = 2 });
      Require(tile.IsCellOpen(cells[0]) && tile.IsCellOpen(cells[1]) && !tile.IsCellOpen(cells[2]), "Legacy completed covers migrate");
      var saved = tile.CaptureProgress();
      string id = cells[1].CellId;
      tile.ConfigureAuthoring(tile.TileId, 99, new List<GameObject> { cells[2].CoverRoot, cells[0].CoverRoot, cells[1].CoverRoot });
      tile.InitializeFromSave(saved);
      Require(cells[1].CellId == id && tile.IsCellOpen(cells[1]) && !tile.IsCellOpen(cells[2]), "Reorder preserves stable identity");
      tile.InitializeFromSave(new MetaTileProgress(tile.TileId) { UsesCellIds = true, OpenedCellIds = new List<string> { "unknown" } });
      Require(!tile.IsTileFullyOpen && !tile.IsCellOpen(cells[0]), "Unknown saved ID does not unlock a cell");
    }

    private static void SelectedCellTransactionRollback(Fixture f)
    {
      var cells = CreateCells(f, out var tile);
      var observer = new MetaObserver(f.Service, f.Save, f.Currency);
      f.Currency.AddHexCoins(31);
      Action<int> fail = _ => throw new InvalidOperationException("Expected currency callback failure");
      f.Currency.OnHexCoinsChanged += fail;
      bool failed = false;
      try { observer.TryOpenCell(tile, cells[1]); }
      catch (InvalidOperationException) { failed = true; }
      finally { f.Currency.OnHexCoinsChanged -= fail; }
      Require(failed && f.Currency.HexCoins == 31 && !tile.IsCellOpen(cells[1]), "Failed transaction restores debit and ID");
      Require(f.Reload().HexCoins == 31 && f.Reload().MetaProgress.Count == 0, "Failed purchase does not persist");
    }

    private static void HexHitGeometry(Fixture f)
    {
      var cells = CreateCells(f, out _);
      var cell = cells[0];
      var renderer = cell.CoverRoot.AddComponent<SpriteRenderer>();
      var texture = new Texture2D(16, 16);
      var sprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), Vector2.one * 0.5f, 8);
      f.Objects.Add(texture);
      f.Objects.Add(sprite);
      renderer.sprite = sprite;
      SetField(cell, "_cover", renderer);
      foreach (var rotation in new[] { Quaternion.identity, Quaternion.Euler(90, 20, 0) })
      {
        cell.transform.localRotation = rotation;
        cell.transform.localScale = new Vector3(2, 3, 1);
        var t = renderer.transform;
        Ray RayAt(float x, float y) => new Ray(t.TransformPoint(new Vector3(x, y, -2)), t.TransformDirection(Vector3.forward));
        Require(cell.TryHit(RayAt(0, 0), out _), "Center hit in XY and rotated plane");
        Require(!cell.TryHit(RayAt(0.95f, 0.95f), out _), "Transparent corner excluded");
        cell.ConfigureHitShape(false);
        Require(cell.TryHit(RayAt(0.9f, 0), out _), "Flat-top tip accepted");
        cell.ConfigureHitShape(true);
      }
    }

    private static void VictoryIsCommittedOnce(Fixture f)
    {
      var config = ScriptableObject.CreateInstance<LevelConfig>();
      f.Objects.Add(config);
      SetField(config, "<WinCondition>k__BackingField", 250);
      SetField(config, "<Difficulty>k__BackingField", LevelDifficulty.Hard);
      f.Factory.CurrentLevelConfig = config;
      f.Factory.CurrentLevelIndex = 0;
      var state = new GameResultState(null, null, null, f.Service, f.Save, f.Factory, f.Currency, new LivesService(f.Service, f.Save));
      var complete = typeof(GameResultState).GetMethod("ApplyResult", BindingFlags.NonPublic | BindingFlags.Instance);
      state.BeginAttempt();
      complete.Invoke(state, new object[] { new GameResultPayload(true, 250) });
      var loaded = f.Reload();
      Require(loaded.LevelIndex == 1 && loaded.Coins == Constants.HardLevelCoinsReward && loaded.HexCoins == 250,
        "Victory saved without Continue button");
      complete.Invoke(state, new object[] { new GameResultPayload(true, 250) });
      Require(f.Reload().LevelIndex == 1 && f.Reload().Coins == Constants.HardLevelCoinsReward,
        "Duplicate result cannot grant another reward");
    }

    private static void DefeatRewardsAndLives(Fixture f)
    {
      var config = ScriptableObject.CreateInstance<LevelConfig>();
      f.Objects.Add(config);
      SetField(config, "<WinCondition>k__BackingField", 250);
      SetField(config, "<Difficulty>k__BackingField", LevelDifficulty.Hard);
      f.Factory.CurrentLevelConfig = config;
      f.Factory.CurrentLevelIndex = 0;
      var lives = new LivesService(f.Service, f.Save);
      var state = new GameResultState(null, null, null, f.Service, f.Save, f.Factory, f.Currency, lives);
      var apply = typeof(GameResultState).GetMethod("ApplyResult", BindingFlags.NonPublic | BindingFlags.Instance);
      state.BeginAttempt();
      apply.Invoke(state, new object[] { new GameResultPayload(false, 100) });
      var saved = f.Reload();
      long deadline = saved.NextLifeUtcTicks;
      Require(saved.LevelIndex == 0 && saved.Coins == 0 && saved.HexCoins == 0 && saved.Lives == 4,
        "Defeat grants no currency and persists one lost life without advancing level");
      Require(deadline > DateTime.UtcNow.Ticks, "Defeat starts recovery deadline");
      apply.Invoke(state, new object[] { new GameResultPayload(false, 100) });
      Require(f.Reload().Lives == 4 && f.Reload().Coins == 0, "Duplicate defeat has no effect");
      state.BeginAttempt();
      apply.Invoke(state, new object[] { new GameResultPayload(false, 0) });
      saved = f.Reload();
      Require(saved.Lives == 3 && saved.Coins == 0 && saved.HexCoins == 0 && saved.NextLifeUtcTicks == deadline,
        "New attempt loses a life without resetting recovery; zero score gives no reward");
    }

    private static void AbandonAttemptSpendsExactlyOnce(Fixture f)
    {
      var config = ScriptableObject.CreateInstance<LevelConfig>();
      f.Objects.Add(config);
      SetField(config, "<WinCondition>k__BackingField", 250);
      f.Factory.CurrentLevelConfig = config;
      f.Factory.CurrentLevelIndex = 0;
      f.Progress.Coins = 42;
      f.Progress.HexCoins = 73;
      var lives = new LivesService(f.Service, f.Save);
      lives.Refresh();
      var state = new GameResultState(null, null, null, f.Service, f.Save, f.Factory, f.Currency, lives);
      Require(!state.TryAbandonAttempt() && lives.Lives == 5, "Cannot abandon without a started attempt");
      state.BeginAttempt();
      Require(lives.Lives == 5, "Starting an attempt does not charge a life");
      Require(state.TryAbandonAttempt(), "Active attempt can be abandoned");
      var saved = f.Reload();
      long deadline = saved.NextLifeUtcTicks;
      Require(saved.Lives == 4 && saved.LevelIndex == 0 && saved.Coins == 42 && saved.HexCoins == 73,
        "Abandon saves one lost life; keeps level and existing balances; grants no rewards");
      Require(!state.TryAbandonAttempt() && f.Reload().Lives == 4, "Double confirmation cannot spend twice");
      state.BeginAttempt();
      Require(state.TryAbandonAttempt() && f.Reload().Lives == 3 && f.Reload().NextLifeUtcTicks == deadline,
        "Next attempt can be abandoned without resetting regeneration");
      state.BeginAttempt();
      var apply = typeof(GameResultState).GetMethod("ApplyResult", BindingFlags.NonPublic | BindingFlags.Instance);
      apply.Invoke(state, new object[] { new GameResultPayload(true, 250) });
      Require(!state.TryAbandonAttempt() && f.Reload().Lives == 3,
        "A completed victory cannot subsequently lose a life through Exit");
    }

    private static void NewerSaveIsNotOverwritten(Fixture f)
    {
      string json = "{\"SaveVersion\":999,\"WorldData\":{\"LastLevel\":{\"Level\":\"Game\"}}}";
      PlayerPrefs.SetString(f.Key, json);
      bool rejected = false;
      try { f.Save.LoadProgress(); } catch (InvalidOperationException) { rejected = true; }
      Require(rejected && PlayerPrefs.GetString(f.Key) == json, "Newer version remains intact");
      rejected = false;
      try { f.Save.SaveProgress(); } catch (InvalidOperationException) { rejected = true; }
      Require(rejected && PlayerPrefs.GetString(f.Key) == json, "Accidental overwrite of newer version refused");
    }

    private static void Require(bool condition, string message)
    {
      if (!condition) throw new Exception("[ProgressSaveChecks] FAIL: " + message);
    }

    private static void SetField(object target, string name, object value) =>
      target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);

    private sealed class Fixture : IDisposable
    {
      public readonly string Key = "HexTest.ProgressChecks." + Guid.NewGuid().ToString("N");
      public readonly PersistentProgressService Service = new PersistentProgressService();
      public readonly FakeFactory Factory = new FakeFactory();
      public readonly List<Object> Objects = new List<Object>();
      public readonly SaveLoadService Save;
      public readonly CurrencyService Currency;
      public PlayerProgress Progress => Service.PlayerProgress;

      public Fixture()
      {
        Service.PlayerProgress = new PlayerProgress(Constants.GameScene);
        Save = new SaveLoadService(Service, Factory, Key);
        Currency = new CurrencyService(Service, Save);
      }

      public PlayerProgress Reload() => new SaveLoadService(Service, Factory, Key).LoadProgress();

      public MetaTile CreateTile()
      {
        var root = new GameObject("ProgressSaveCheckTile") { hideFlags = HideFlags.HideAndDontSave };
        Objects.Add(root);
        var tile = root.AddComponent<MetaTile>();
        var item = new GameObject("Object");
        item.transform.SetParent(root.transform);
        var group = new MetaTileObjectGroup();
        group.Objects.Add(item);
        SetField(tile, "_tileId", "test-tile");
        SetField(tile, "_groups", new List<MetaTileObjectGroup> { group });
        SetField(tile, "_progressPerStep", 0.25f);
        tile.InitializeFromSave(null);
        return tile;
      }

      public void Dispose()
      {
        foreach (var obj in Objects) if (obj != null) Object.DestroyImmediate(obj);
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.DeleteKey(Key + ".Backup");
        PlayerPrefs.Save();
      }
    }

    private sealed class FakeFactory : IGameFactory
    {
      public LevelConfig CurrentLevelConfig { get; set; }
      public int CurrentLevelIndex { get; set; } = -1;
      public List<IProgressReader> ProgressReaders { get; } = new List<IProgressReader>();
      public List<IProgressSaver> ProgressSavers { get; } = new List<IProgressSaver>();
      public ObjectPool<StackTile> CreateCellPool() => throw new NotSupportedException();
      public GridSpawner CreateGridSpawner(ObjectPool<StackTile> pool, MainMenuObserver menu, int index) => throw new NotSupportedException();
      public StacksSpawner CreateStacksSpawner(ObjectPool<StackTile> pool, HexGrid grid) => throw new NotSupportedException();
      public void Clear() { }
    }
  }
}
