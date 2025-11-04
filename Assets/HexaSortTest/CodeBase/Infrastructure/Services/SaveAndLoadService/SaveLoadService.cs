using CodeBase.Infrastructure;
using CodeBase.Infrastructure.Services.Factories;
using HexaSortTest.CodeBase.GameLogic.Data;
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
      foreach (IProgressSaver progressSaver in _gameFactory.ProgressSavers)
        progressSaver.UpdateProgress(_progressService.PlayerProgress);
      
      PlayerPrefs.SetString(Constants.ProgressKey, _progressService.PlayerProgress.ToJson());
    }

    public PlayerProgress LoadProgress() =>
      PlayerPrefs.GetString(Constants.ProgressKey)?
        .ToDeserialized<PlayerProgress>();
  }
}