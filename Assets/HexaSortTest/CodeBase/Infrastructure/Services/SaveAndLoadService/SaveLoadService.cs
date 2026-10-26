using System;
using System.Collections.Generic;
using HexaSortTest.CodeBase.GameLogic.Boosters;
using HexaSortTest.CodeBase.GameLogic.Data;
using HexaSortTest.CodeBase.Infrastructure.Logging;
using HexaSortTest.CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;
using Zenject;

namespace HexaSortTest.CodeBase.Infrastructure.Services.SaveAndLoadService
{
  public class SaveLoadService : ISaveLoadService
  {
    private readonly IPersistentProgressService _progressService;
    private readonly IGameFactory _gameFactory;
    private readonly string _progressKey;
    private string BackupKey => _progressKey + ".Backup";
    private int _transactionDepth;

    [Inject]
    public SaveLoadService(IPersistentProgressService progressService, IGameFactory gameFactory)
      : this(progressService, gameFactory, Constants.ProgressKey) { }

    // Separate keys allow editor checks without touching the player's profile.
    public SaveLoadService(IPersistentProgressService progressService, IGameFactory gameFactory, string progressKey)
    {
      _progressService = progressService;
      _gameFactory = gameFactory;
      _progressKey = progressKey;
    }

    public void ExecuteTransaction(Action update)
    {
      if (update == null) throw new ArgumentNullException(nameof(update));
      var progress = _progressService.PlayerProgress;
      if (progress == null) throw new InvalidOperationException("Progress has not been loaded.");

      string before = progress.ToJson();
      bool updateFinished = false;
      _transactionDepth++;
      try
      {
        update();
        _transactionDepth--;
        updateFinished = true;
        if (_transactionDepth == 0) SaveProgress();
      }
      catch
      {
        if (!updateFinished) _transactionDepth--;
        // Keep the root instance: services/UI may already hold a reference to it.
        JsonUtility.FromJsonOverwrite(before, progress);
        _progressService.PlayerProgress = progress;
        throw;
      }
    }

    public void SaveProgress()
    {
      var progress = _progressService.PlayerProgress;
      if (progress == null || _transactionDepth > 0) return;

      foreach (IProgressSaver saver in _gameFactory.ProgressSavers)
      {
        if (saver == null || (saver is UnityEngine.Object obj && obj == null)) continue;
        saver.UpdateProgress(progress);
      }
      progress.SaveVersion = PlayerProgress.CurrentSaveVersion;
      string json = progress.ToJson();
      string previous = PlayerPrefs.GetString(_progressKey, string.Empty);
      string backup = PlayerPrefs.GetString(BackupKey, string.Empty);

      // Repeated pause/focus events must not replace the older recovery point.
      if (json == previous)
      {
        if (!TryDeserialize(backup, out _)) PlayerPrefs.SetString(BackupKey, json);
        PlayerPrefs.Save();
        return;
      }

      if (TryDeserialize(previous, out _))
        PlayerPrefs.SetString(BackupKey, previous);
      else if (!TryDeserialize(backup, out _))
        PlayerPrefs.SetString(BackupKey, json);

      PlayerPrefs.SetString(_progressKey, json);
      PlayerPrefs.Save();
#if UNITY_EDITOR
      GameLogger.Message($"[SaveLoadService] Progress flushed to disk; level index {progress.LevelIndex}.");
#endif
    }

    public PlayerProgress LoadProgress()
    {
      if (TryDeserialize(PlayerPrefs.GetString(_progressKey, string.Empty), out var progress))
        return progress;

      if (TryDeserialize(PlayerPrefs.GetString(BackupKey, string.Empty), out progress))
      {
#if UNITY_EDITOR
        GameLogger.Warning("[SaveLoadService] Main save missing or invalid; recovered the previous saved state.");
#endif
        return progress;
      }
#if UNITY_EDITOR
      GameLogger.Message("[SaveLoadService] No usable save; a new profile is required.");
#endif
      return null;
    }

    private bool TryDeserialize(string json, out PlayerProgress progress)
    {
      progress = null;
      if (string.IsNullOrWhiteSpace(json)) return false;
      try
      {
        var loaded = json.ToDeserialized<PlayerProgress>();
        if (loaded == null) return false;
        if (loaded.SaveVersion > PlayerProgress.CurrentSaveVersion)
          throw new InvalidOperationException("This save was created by a newer game version; update the game to load it.");
        // Reject syntactically valid but unrelated JSON such as {}.
        if (loaded.WorldData?.LastLevel == null ||
            string.IsNullOrWhiteSpace(loaded.WorldData.LastLevel.Level)) return false;

        // Version 0 is the original unversioned PlayerPrefs format.
        loaded.SaveVersion = PlayerProgress.CurrentSaveVersion;
        loaded.WorldData.LastLevel.Level = Constants.GameScene;
        loaded.LevelIndex = Math.Max(0, loaded.LevelIndex);
        loaded.Coins = Math.Max(0, loaded.Coins);
        loaded.HexCoins = Math.Max(0, loaded.HexCoins);
        loaded.Boosters ??= new List<BoosterInventoryEntry>();
        loaded.MetaProgress ??= new List<MetaTileProgress>();

        var boosterTypes = new HashSet<BoosterType>();
        loaded.Boosters.RemoveAll(entry => entry == null ||
          !Enum.IsDefined(typeof(BoosterType), entry.Type) || !boosterTypes.Add(entry.Type));
        foreach (var entry in loaded.Boosters) entry.Count = Math.Max(0, entry.Count);

        var tileIds = new HashSet<string>();
        loaded.MetaProgress.RemoveAll(entry => entry == null ||
          string.IsNullOrWhiteSpace(entry.TileId) || !tileIds.Add(entry.TileId));
        foreach (var entry in loaded.MetaProgress)
        {
          entry.UnlockedGroupsCount = Math.Max(0, entry.UnlockedGroupsCount);
          entry.UnlockedObjectsInGroupCount = Math.Max(0, entry.UnlockedObjectsInGroupCount);
          entry.CurrentObjectProgress = float.IsNaN(entry.CurrentObjectProgress) ||
            float.IsInfinity(entry.CurrentObjectProgress) ? 0f : Mathf.Clamp01(entry.CurrentObjectProgress);
        }
        progress = loaded;
        return true;
      }
      catch (ArgumentException)
      {
        return false;
      }
    }
  }
}
