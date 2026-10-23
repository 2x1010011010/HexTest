using System.Collections.Generic;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Cells
{
  // Authored blocker view only. No damage, hit counting or destruction behavior.
  [DisallowMultipleComponent]
  [RequireComponent(typeof(SpriteRenderer))]
  public sealed class Blocker : MonoBehaviour
  {
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField, Tooltip("Required Hits 1 uses element 0, 2 uses element 1, etc.")]
    private List<Sprite> _spriteList = new();
    [SerializeField, Min(1)] private int _requiredHits = 1;

    public SpriteRenderer SpriteRenderer => _spriteRenderer;
    public IReadOnlyList<Sprite> SpriteList => _spriteList;
    public int RequiredHits => _requiredHits;

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
    }

    public void RefreshEditorSprite()
    {
      if (_spriteRenderer == null)
        return;
      TryGetSprite(_requiredHits, out Sprite sprite);
      // Missing entries stay visibly missing instead of displaying a stale stage.
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
