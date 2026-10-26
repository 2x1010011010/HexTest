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
      Run(VictoryIsCommittedOnce, ref count);
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

    private static void VictoryIsCommittedOnce(Fixture f)
    {
      var config = ScriptableObject.CreateInstance<LevelConfig>();
      f.Objects.Add(config);
      SetField(config, "<WinCondition>k__BackingField", 250);
      SetField(config, "<Difficulty>k__BackingField", LevelDifficulty.Hard);
      f.Factory.CurrentLevelConfig = config;
      f.Factory.CurrentLevelIndex = 0;
      var state = new GameResultState(null, null, null, f.Service, f.Save, f.Factory, f.Currency);
      var complete = typeof(GameResultState).GetMethod("CompleteLevel", BindingFlags.NonPublic | BindingFlags.Instance);
      complete.Invoke(state, null);
      var loaded = f.Reload();
      Require(loaded.LevelIndex == 1 && loaded.Coins == Constants.HardLevelCoinsReward && loaded.HexCoins == 250,
        "Victory saved without Continue button");
      complete.Invoke(state, null);
      Require(f.Reload().LevelIndex == 1 && f.Reload().Coins == Constants.HardLevelCoinsReward,
        "Duplicate result cannot grant another reward");
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
