using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Cells
{
  [DisallowMultipleComponent]
  [RequireComponent(typeof(SpriteRenderer))]
  public sealed class Blocker : MonoBehaviour
  {
    [SerializeField, BoxGroup("BLOCKER RENDER")] private SpriteRenderer _spriteRenderer;
    [SerializeField, Tooltip("Required Hits 1 uses element 0, 2 uses element 1, etc."), BoxGroup("BLOCKER SETUP")]
    private List<Sprite> _spriteList = new();
    [SerializeField, Min(1), BoxGroup("BLOCKER SETUP")] private int _requiredHits = 1;

    [SerializeField, BoxGroup("FX")] private ParticleSystem _hitParticles;
    public ParticleSystem HitParticles => _hitParticles;

    public SpriteRenderer SpriteRenderer => _spriteRenderer;
    public IReadOnlyList<Sprite> SpriteList => _spriteList;
    public int RequiredHits => _requiredHits;

    public void SetRemainingHits(int remaining)
    {
      if (_spriteRenderer == null)
        _spriteRenderer = GetComponent<SpriteRenderer>();
      TryGetSprite(remaining, out Sprite sprite);
      _spriteRenderer.sprite = sprite;
    }

    public bool TryGetSprite(int requiredHits, out Sprite sprite)
    {
      sprite = null;
      if (_spriteList == null || requiredHits < 1 || requiredHits > _spriteList.Count)
        return false;
      sprite = _spriteList[requiredHits - 1];
      return sprite != null;
    }

#if UNITY_EDITOR
    public void ConfigureForEditor(int requiredHits)
    {
      _requiredHits = Mathf.Max(1, requiredHits);
      RefreshEditorSprite();
    }

    public void AssignMissingEditorReferences()
    {
      if (_spriteRenderer == null)
        _spriteRenderer = GetComponent<SpriteRenderer>();
      if (_hitParticles == null)
        _hitParticles = GetComponentInChildren<ParticleSystem>(true);
    }

    public void RefreshEditorSprite()
    {
      if (_spriteRenderer == null)
        return;
      TryGetSprite(_requiredHits, out Sprite sprite);
      _spriteRenderer.sprite = sprite;
    }

    private void Reset()
    {
      AssignMissingEditorReferences();
      RefreshEditorSprite();
    }

    private void OnValidate() => _requiredHits = Mathf.Max(1, _requiredHits);
#endif
  }
}
