using HexaSortTest.CodeBase.GameLogic.Data;
using HexaSortTest.CodeBase.Infrastructure.Logging;
using HexaSortTest.CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.SaveAndLoadService
{
  public class SaveLoadService : ISaveLoadService
  {
    private readonly IPersistentProgressService _progressService;
    private readonly IGameFactory _gameFactory;

    public SaveLoadService(IPersistentProgressService progressService, IGameFactory gameFactory)
    {
      _progressService = progressService;
      _gameFactory = gameFactory;
    }

    public void SaveProgress()
    {
#if UNITY_EDITOR
      GameLogger.Message($"[SaveLoadService] Saving progress; {_gameFactory.ProgressSavers.Count} savers.");
#endif
      foreach (IProgressSaver progressSaver in _gameFactory.ProgressSavers)
        progressSaver.UpdateProgress(_progressService.PlayerProgress);
      
      PlayerPrefs.SetString(Constants.ProgressKey, _progressService.PlayerProgress.ToJson());
#if UNITY_EDITOR
      GameLogger.Message($"[SaveLoadService] Progress written to PlayerPrefs; level index {_progressService.PlayerProgress.LevelIndex}.");
#endif
    }

    public PlayerProgress LoadProgress()
    {
#if UNITY_EDITOR
      GameLogger.Message("[SaveLoadService] Reading progress from PlayerPrefs.");
#endif
      var progress = PlayerPrefs.GetString(Constants.ProgressKey)?
        .ToDeserialized<PlayerProgress>();
#if UNITY_EDITOR
      GameLogger.Message(progress == null ? "[SaveLoadService] Saved progress is absent or empty." : "[SaveLoadService] Progress deserialized.");
#endif
      return progress;
    }
  }
}