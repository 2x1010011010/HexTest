using System;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using HexaSortTest.CodeBase.Infrastructure.Services.SaveAndLoadService;

namespace HexaSortTest.CodeBase.Infrastructure.Services.LivesService
{
  // UTC deadline survives scene changes and time spent outside the application.
  public class LivesService
  {
    public const int MaxLives = 5;
    private static readonly long IntervalTicks = TimeSpan.FromMinutes(30).Ticks;
    private readonly IPersistentProgressService _progress;
    private readonly ISaveLoadService _save;
    public event Action<int> OnLivesChanged;
    public int Lives => _progress.PlayerProgress.Lives;
    public TimeSpan TimeToNextLife => Lives >= MaxLives ? TimeSpan.Zero :
      TimeSpan.FromTicks(Math.Max(0, _progress.PlayerProgress.NextLifeUtcTicks - DateTime.UtcNow.Ticks));

    public LivesService(IPersistentProgressService progress, ISaveLoadService save)
    {
      _progress = progress;
      _save = save;
    }

    public void Refresh()
    {
      var data = _progress.PlayerProgress;
      if (data == null) return;
      int before = data.Lives;
      long deadlineBefore = data.NextLifeUtcTicks;
      bool initialize = !data.LivesInitialized;
      long now = DateTime.UtcNow.Ticks;
      if (initialize)
      {
        data.LivesInitialized = true;
        data.Lives = MaxLives;
        data.NextLifeUtcTicks = 0;
      }
      data.Lives = Math.Max(0, Math.Min(MaxLives, data.Lives));
      if (data.Lives >= MaxLives)
        data.NextLifeUtcTicks = 0;
      else if (data.NextLifeUtcTicks <= 0)
        data.NextLifeUtcTicks = now + IntervalTicks;
      else if (now >= data.NextLifeUtcTicks)
      {
        int restored = (int)Math.Min(MaxLives - data.Lives,
          1 + (now - data.NextLifeUtcTicks) / IntervalTicks);
        data.Lives += restored;
        data.NextLifeUtcTicks = data.Lives >= MaxLives ? 0 :
          data.NextLifeUtcTicks + restored * IntervalTicks;
      }
      if (initialize || before != data.Lives || deadlineBefore != data.NextLifeUtcTicks)
      {
        _save.SaveProgress();
        OnLivesChanged?.Invoke(data.Lives);
      }
    }

    public bool TrySpendLife()
    {
      Refresh();
      if (Lives <= 0) return false;
      var data = _progress.PlayerProgress;
      if (data.Lives == MaxLives)
        data.NextLifeUtcTicks = DateTime.UtcNow.Ticks + IntervalTicks;
      data.Lives--;
      _save.SaveProgress();
      OnLivesChanged?.Invoke(data.Lives);
      return true;
    }

    public void AddLives(int amount)
    {
      Refresh();
      if (amount <= 0 || Lives >= MaxLives) return;
      var data = _progress.PlayerProgress;
      data.Lives += Math.Min(amount, MaxLives - data.Lives);
      if (data.Lives == MaxLives) data.NextLifeUtcTicks = 0;
      _save.SaveProgress();
      OnLivesChanged?.Invoke(data.Lives);
    }
  }
}
