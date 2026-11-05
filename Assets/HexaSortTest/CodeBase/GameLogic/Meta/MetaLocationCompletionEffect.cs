using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Meta
{
  [DisallowMultipleComponent]
  public sealed class MetaLocationCompletionEffect : MonoBehaviour
  {
    [SerializeField, Min(0.01f)] private float _shakeDuration = 0.4f;
    [SerializeField, Min(0f)] private float _shakeAngle = 9f;
    [SerializeField, Min(0.01f)] private float _fadeDuration = 0.35f;

    private sealed class FrameState
    {
      public SpriteRenderer Renderer;
      public Quaternion Rotation;
      public Color Color;
      public bool Enabled;
    }
    private sealed class MaskState
    {
      public SpriteMask Mask;
      public bool Enabled;
    }
    private sealed class PictureState
    {
      public SpriteRenderer Renderer;
      public SpriteMaskInteraction Interaction;
    }

    private readonly List<FrameState> _frames = new();
    private readonly List<MaskState> _masks = new();
    private readonly List<PictureState> _pictures = new();
    private List<MetaLocationCell> _cells;
    private MetaTile _tile;
    private Coroutine _waiting;
    private Sequence _sequence;
    private bool _captured, _requested, _finished;

    // Called after restoring progress, including transaction rollback.
    public void Restore(MetaTile tile)
    {
      Cancel();
      _tile = tile;
      if (!_captured) Capture();
      _requested = _finished = false;
      foreach (var state in _frames)
      {
        if (state.Renderer == null) continue;
        state.Renderer.transform.localRotation = state.Rotation;
        state.Renderer.color = state.Color;
        state.Renderer.enabled = state.Enabled;
      }
      foreach (var state in _masks)
        if (state.Mask != null) state.Mask.enabled = state.Enabled;
      foreach (var state in _pictures)
        if (state.Renderer != null) state.Renderer.maskInteraction = state.Interaction;
      if (tile.IsTileFullyOpen) Finish();
    }

    private void Capture()
    {
      _captured = true;
      _cells = _tile.Cells;
      var frames = new HashSet<SpriteRenderer>();
      var masks = new HashSet<SpriteMask>();
      foreach (var cell in _cells)
      {
        if (cell == null) continue;
        if (cell.Frame != null && frames.Add(cell.Frame))
          _frames.Add(new FrameState { Renderer = cell.Frame, Rotation = cell.Frame.transform.localRotation,
            Color = cell.Frame.color, Enabled = cell.Frame.enabled });
        if (cell.Mask != null && masks.Add(cell.Mask))
          _masks.Add(new MaskState { Mask = cell.Mask, Enabled = cell.Mask.enabled });
      }
      // The picture is outside the generated cells. Never unmask covers or locks.
      foreach (var sprite in _tile.GetComponentsInChildren<SpriteRenderer>(true))
        if (sprite.GetComponentInParent<MetaLocationCell>() == null &&
            sprite.maskInteraction == SpriteMaskInteraction.VisibleInsideMask)
          _pictures.Add(new PictureState { Renderer = sprite, Interaction = sprite.maskInteraction });
    }

    public void Play()
    {
      if (_tile == null || !_tile.IsTileFullyOpen || _requested || _finished) return;
      _requested = true;
      if (!isActiveAndEnabled) { Finish(); return; }
      _waiting = StartCoroutine(WaitForCovers());
    }

    private IEnumerator WaitForCovers()
    {
      // Purchase completion is emitted before the debit and lock animations finish.
      // Also wait for earlier purchases if the player opened several cells quickly.
      yield return null;
      while (HasVisibleCovers()) yield return null;
      _waiting = null;
      if (_tile == null || !_tile.IsTileFullyOpen) yield break;
      if (_frames.Count == 0) { Finish(); yield break; }

      float shake = Mathf.Max(0.01f, _shakeDuration);
      float fade = Mathf.Max(0.01f, _fadeDuration);
      _sequence = DOTween.Sequence().SetUpdate(true);
      _sequence.AppendInterval(shake + fade);
      foreach (var state in _frames)
      {
        var frame = state.Renderer;
        if (frame == null || !frame.enabled || !frame.gameObject.activeInHierarchy) continue;
        _sequence.Insert(0f, frame.transform.DOShakeRotation(shake,
          new Vector3(0f, 0f, _shakeAngle), 8, 0f, true));
        _sequence.Insert(shake, frame.DOFade(0f, fade).SetEase(Ease.InQuad));
      }
      _sequence.OnComplete(() => { _sequence = null; Finish(); });
    }

    private bool HasVisibleCovers()
    {
      foreach (var cell in _cells)
        if (cell != null && cell.CoverRoot != null && cell.CoverRoot.activeInHierarchy) return true;
      return false;
    }

    private void Finish()
    {
      _finished = true;
      // Disabling SpriteMask alone would hide a VisibleInsideMask picture.
      foreach (var state in _pictures)
        if (state.Renderer != null) state.Renderer.maskInteraction = SpriteMaskInteraction.None;
      foreach (var state in _masks)
        if (state.Mask != null) state.Mask.enabled = false;
      foreach (var state in _frames)
      {
        if (state.Renderer == null) continue;
        state.Renderer.transform.localRotation = state.Rotation;
        state.Renderer.enabled = false;
      }
    }

    private void Cancel()
    {
      if (_waiting != null) StopCoroutine(_waiting);
      _waiting = null;
      _sequence?.Kill();
      _sequence = null;
    }

    private void OnDisable()
    {
      Cancel();
      if (_captured && _tile != null && _tile.IsTileFullyOpen) Finish();
    }
  }
}
