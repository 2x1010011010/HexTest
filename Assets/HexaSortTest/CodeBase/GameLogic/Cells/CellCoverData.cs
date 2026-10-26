using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using HexaSortTest.CodeBase.Infrastructure.Logging;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using Sirenix.OdinInspector;

namespace HexaSortTest.CodeBase.GameLogic.Cells
{
  public sealed class CellCoverData : MonoBehaviour
  {
    [SerializeField, BoxGroup("CELL SETUP")] private CellCoverType _type;
    [SerializeField, Min(0), BoxGroup("CELL SETUP")] private int _requiredScore;
    [SerializeField, Min(0), BoxGroup("CELL SETUP")] private int _requiredHits;
    [SerializeField, BoxGroup("CELL SETUP")] private GameObject _visualRoot;
    [SerializeField, BoxGroup("CELL SETUP")] private SpriteRenderer _coverRenderer;

    public CellCoverType Type => _type;
    // ScoreLocker is the authored source for visible locks. Keep the cell field
    // only as a fallback for old levels without a ScoreLocker component.
    public int RequiredScore => Locker != null ? Locker.RequiredScore : _requiredScore;
    private ScoreLocker Locker => _locker != null ? _locker :
      (_locker = _visualRoot != null ? _visualRoot.GetComponentInChildren<ScoreLocker>(true) : null);
    public int RequiredHits => _type == CellCoverType.BreakableBlocker ? Mathf.Max(1, _requiredHits) : 0;
    public GameObject VisualRoot => _visualRoot;
    public SpriteRenderer CoverRenderer => _coverRenderer;

    private Cell _cell;
    private ScoreLocker _locker;
    private Blocker _blocker;
    private int _remainingHits;
    private bool _unlocked;
    public bool IsUnlocking { get; private set; }
    public bool IsBlocked => _type != CellCoverType.Normal && !_unlocked;

    private void Awake()
    {
      _cell = GetComponent<Cell>();
      _locker = _visualRoot != null ? _visualRoot.GetComponentInChildren<ScoreLocker>(true) : null;
      _blocker = _visualRoot != null ? _visualRoot.GetComponentInChildren<Blocker>(true) : null;
      _remainingHits = RequiredHits;
      SetCellLayer(LayerMask.NameToLayer(IsBlocked ? "Default" : "Grid"));
      if (_visualRoot != null)
        foreach (var particles in _visualRoot.GetComponentsInChildren<ParticleSystem>(true))
          particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
      if (IsBlocked)
      {
        if (_blocker != null)
          _blocker.SetRemainingHits(_remainingHits);
      }
    }

    public async UniTask UpdateScoreAsync(int score, CancellationToken token)
    {
      if (_type != CellCoverType.ScoreLock || _unlocked || IsUnlocking)
        return;
      int threshold = RequiredScore;
      int remaining = Mathf.Max(0, threshold - Mathf.Max(0, score));
      if (_locker != null)
        _locker.SetRemainingScore(remaining);
      if (remaining == 0)
      {
#if UNITY_EDITOR
        GameLogger.Message($"[ScoreLock] Cell={name} position={transform.position}, " +
          $"visual={(_visualRoot != null ? _visualRoot.name : "missing")}, " +
          $"threshold={threshold}, score={score}. Starting unlock.");
#endif
        await UnlockAsync(_locker != null ? _locker.UnlockParticles : null, token);
      }
    }

    public async UniTask HitAsync(CancellationToken token)
    {
      if (_type != CellCoverType.BreakableBlocker || _unlocked || IsUnlocking)
        return;
      _remainingHits = Mathf.Max(0, _remainingHits - 1);
      if (_blocker != null)
        _blocker.SetRemainingHits(_remainingHits);
      ParticleSystem particles = _blocker != null ? _blocker.HitParticles : null;
      if (_remainingHits == 0)
        await UnlockAsync(particles, token);
      else
        await PlayParticlesAsync(particles, token);
    }

    private async UniTask UnlockAsync(ParticleSystem particles, CancellationToken token)
    {
      IsUnlocking = true;
      await PlayParticlesAsync(particles, token);
      if (this == null)
        return;
      _unlocked = true;
      IsUnlocking = false;
      if (_visualRoot != null)
        _visualRoot.SetActive(false);
      SetCellLayer(LayerMask.NameToLayer("Grid"));
      _cell.SetEmpty(_cell.GetComponentInChildren<Stack>() == null);
#if UNITY_EDITOR
      GameLogger.Message($"[CellCoverData] Unlocked {name} ({_type}).");
#endif
    }

    private async UniTask PlayParticlesAsync(ParticleSystem particles, CancellationToken token)
    {
      if (particles == null)
        return;
      particles.gameObject.SetActive(true);
      particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
      foreach (var system in particles.GetComponentsInChildren<ParticleSystem>(true))
      {
        var main = system.main;
        main.loop = false;
        main.stopAction = ParticleSystemStopAction.None;
      }
      particles.Play(true);
      await UniTask.WaitUntil(() => particles == null || !particles.IsAlive(true),
        cancellationToken: token);
    }

    private void SetCellLayer(int layer)
    {
      if (layer < 0)
        return;
      gameObject.layer = layer;
      foreach (var collider in GetComponentsInChildren<Collider>(true))
        collider.gameObject.layer = layer;
    }

    public void Initialize(CellCoverType type, int requiredScore, int requiredHits, GameObject visualRoot)
    {
      _type = type;
      _requiredScore = type == CellCoverType.ScoreLock ? Mathf.Max(0, requiredScore) : 0;
      _requiredHits = type == CellCoverType.BreakableBlocker ? Mathf.Max(1, requiredHits) : 0;
      _visualRoot = visualRoot;
      _cell = GetComponent<Cell>();
      _locker = visualRoot != null ? visualRoot.GetComponentInChildren<ScoreLocker>(true) : null;
      if (_type == CellCoverType.ScoreLock && _locker != null)
        _locker.ConfigureRequiredScore(_requiredScore);
      _coverRenderer = visualRoot != null ? visualRoot.GetComponentInChildren<SpriteRenderer>(true) : null;
    }
  }
}
