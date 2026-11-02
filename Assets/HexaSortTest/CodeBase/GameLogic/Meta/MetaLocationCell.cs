using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Meta
{
  [DisallowMultipleComponent]
  public class MetaLocationCell : MonoBehaviour
  {
    [SerializeField, Min(0), LabelText("HexCoins Cost")]
    private int _hexCoinsCost = 10;
    [SerializeField, ReadOnly] private GameObject _coverRoot;
    [SerializeField, ReadOnly] private SpriteRenderer _frame;
    [SerializeField, ReadOnly] private SpriteMask _mask;
    [SerializeField, ReadOnly] private SpriteRenderer _cover;
    [SerializeField, ReadOnly] private SpriteRenderer _lock;
    [SerializeField, ReadOnly] private TextMeshPro _priceLabel;

    public int HexCoinsCost => _hexCoinsCost;
    public GameObject CoverRoot => _coverRoot;
    public SpriteRenderer Frame => _frame;
    public SpriteMask Mask => _mask;
    public SpriteRenderer Cover => _cover;
    public SpriteRenderer Lock => _lock;
    public TextMeshPro PriceLabel => _priceLabel;
    public bool HasVisuals => _coverRoot != null && _frame != null && _mask != null &&
                              _cover != null && _lock != null && _priceLabel != null;

    private void Awake() => RefreshPrice();
    private void OnValidate()
    {
      _hexCoinsCost = Mathf.Max(0, _hexCoinsCost);
      // TMP mesh updates are deferred to its normal update; never regenerate hierarchy here.
      RefreshPrice();
    }
    public void RefreshPrice()
    {
      if (_priceLabel != null) _priceLabel.text = _hexCoinsCost.ToString();
    }

#if UNITY_EDITOR
    public void Configure(int cost, GameObject coverRoot, SpriteRenderer frame, SpriteMask mask,
      SpriteRenderer cover, SpriteRenderer lockRenderer, TextMeshPro priceLabel)
    {
      _hexCoinsCost = Mathf.Max(0, cost);
      _coverRoot = coverRoot;
      _frame = frame;
      _mask = mask;
      _cover = cover;
      _lock = lockRenderer;
      _priceLabel = priceLabel;
      RefreshPrice();
    }
#endif
  }
}
