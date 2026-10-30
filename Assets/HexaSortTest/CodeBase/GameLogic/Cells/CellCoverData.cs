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

    // Keep the async API for existing callers, but commit gameplay synchronously.
    public UniTask UpdateScoreAsync(int score, CancellationToken token)
    {
      token.ThrowIfCancellationRequested();
      if (_type != CellCoverType.ScoreLock || _unlocked)
        return UniTask.CompletedTask;
      int remaining = Mathf.Max(0, RequiredScore - Mathf.Max(0, score));
      if (Locker != null) Locker.SetRemainingScore(remaining);
      if (remaining == 0) Unlock(_locker != null ? _locker.UnlockParticles : null);
      return UniTask.CompletedTask;
    }

    public UniTask HitAsync(CancellationToken token)
    {
      token.ThrowIfCancellationRequested();
      if (_type != CellCoverType.BreakableBlocker || _unlocked)
        return UniTask.CompletedTask;
      _remainingHits = Mathf.Max(0, _remainingHits - 1);
      if (_blocker != null) _blocker.SetRemainingHits(_remainingHits);
      var particles = _blocker != null ? _blocker.HitParticles : null;
      if (_remainingHits == 0) Unlock(particles);
      else PlayParticles(particles);
      return UniTask.CompletedTask;
    }

    private void Unlock(ParticleSystem particles)
    {
      // No particle lifetime participates in cell state or the board's operation queue.
      _unlocked = true;
      IsUnlocking = false;
      SetCellLayer(LayerMask.NameToLayer("Grid"));
      if (_cell != null) _cell.SetEmpty(_cell.GetComponentInChildren<Stack>() == null);
      HideCover(particles);
      PlayParticles(particles);
#if UNITY_EDITOR
      GameLogger.Message($"[CellCoverData] Unlocked {name} ({_type}).");
#endif
    }

    private void HideCover(ParticleSystem particles)
    {
      if (_visualRoot == null) return;
      // Keep the effect under the cell (and scene lifetime), outside the hidden cover.
      if (particles != null && particles.gameObject != _visualRoot &&
          particles.transform.IsChildOf(_visualRoot.transform))
        particles.transform.SetParent(transform, true);

      if (particles == null || !particles.transform.IsChildOf(_visualRoot.transform))
      {
        _visualRoot.SetActive(false);
        return;
      }

      // Also support prefabs with the ParticleSystem on the cover root itself.
      foreach (var renderer in _visualRoot.GetComponentsInChildren<Renderer>(true))
        if (!(renderer is ParticleSystemRenderer)) renderer.enabled = false;
      foreach (var canvas in _visualRoot.GetComponentsInChildren<Canvas>(true)) canvas.enabled = false;
      foreach (var collider in _visualRoot.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
    }

    private void PlayParticles(ParticleSystem particles)
    {
      if (particles == null) return;
      try
      {
        particles.gameObject.SetActive(true);
        foreach (var system in particles.GetComponentsInChildren<ParticleSystem>(true))
        {
          var main = system.main;
          main.loop = false;
          main.stopAction = ParticleSystemStopAction.None;
        }
        // Restart emission for repeated hits without clearing already emitted particles.
        particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        particles.Play(true);
      }
      catch (System.Exception error)
      {
        // A cosmetic failure must never fault the gameplay session.
#if UNITY_EDITOR
        GameLogger.Error($"[CellCoverData] VFX failed on {name}: {error}");
#endif
      }
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
