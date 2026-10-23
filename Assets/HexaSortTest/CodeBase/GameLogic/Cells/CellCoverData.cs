using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Cells
{
  // Authored level data only. Unlocking and damage behavior will be added separately.
  public sealed class CellCoverData : MonoBehaviour
  {
    [SerializeField] private CellCoverType _type;
    [SerializeField, Min(0)] private int _requiredScore;
    [SerializeField] private SpriteRenderer _coverRenderer;

    public CellCoverType Type => _type;
    public int RequiredScore => _requiredScore;
    public SpriteRenderer CoverRenderer => _coverRenderer;

    public void Initialize(CellCoverType type, int requiredScore, SpriteRenderer coverRenderer)
    {
      _type = type;
      _requiredScore = type == CellCoverType.ScoreLock ? Mathf.Max(0, requiredScore) : 0;
      _coverRenderer = coverRenderer;
    }
  }
}
