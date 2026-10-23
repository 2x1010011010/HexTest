using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Cells
{
  // Authored level data only. Unlocking and damage behavior will be added separately.
  public sealed class CellCoverData : MonoBehaviour
  {
    [SerializeField] private CellCoverType _type;
    [SerializeField, Min(0)] private int _requiredScore;
    [SerializeField, Min(0)] private int _requiredHits;
    [SerializeField] private GameObject _visualRoot;
    [SerializeField] private SpriteRenderer _coverRenderer;

    public CellCoverType Type => _type;
    public int RequiredScore => _requiredScore;
    public int RequiredHits => _type == CellCoverType.BreakableBlocker ? Mathf.Max(1, _requiredHits) : 0;
    public GameObject VisualRoot => _visualRoot;
    public SpriteRenderer CoverRenderer => _coverRenderer;

    public void Initialize(CellCoverType type, int requiredScore, int requiredHits, GameObject visualRoot)
    {
      _type = type;
      _requiredScore = type == CellCoverType.ScoreLock ? Mathf.Max(0, requiredScore) : 0;
      _requiredHits = type == CellCoverType.BreakableBlocker ? Mathf.Max(1, requiredHits) : 0;
      _visualRoot = visualRoot;
      _coverRenderer = visualRoot != null ? visualRoot.GetComponentInChildren<SpriteRenderer>(true) : null;
    }
  }
}
