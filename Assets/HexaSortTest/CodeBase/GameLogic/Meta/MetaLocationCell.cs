using Sirenix.OdinInspector;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Meta
{
  [DisallowMultipleComponent]
  public class MetaLocationCell : MonoBehaviour
  {
    [SerializeField, ReadOnly] private string _cellId;
    [SerializeField, ReadOnly] private bool _pointyTop = true;
    [SerializeField, Tooltip("Optional effect on a persistent child, outside Cover.")] private ParticleSystem _openParticles;
    [SerializeField, Min(0.01f)] private float _lockShakeDuration = 0.35f;
    [SerializeField, Min(0f)] private float _lockShakeAngle = 18f;
    private Tween _revealTween;
    private bool _paid;
    private Quaternion _lockRotation;
    private bool _hasLockRotation;
    public string CellId => _cellId;

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

    public void SetOpened(bool opened)
    {
      _paid = opened;
      _revealTween?.Kill();
      _revealTween = null;
      RestoreLock();
      if (_coverRoot != null) _coverRoot.SetActive(!opened);
    }

    public void PlayReveal()
    {
      _paid = true;
      _revealTween?.Kill();
      RestoreLock();
      if (_lock == null) { FinishReveal(true); return; }
      _lockRotation = _lock.transform.localRotation;
      _hasLockRotation = true;
      _revealTween = _lock.transform.DOShakeRotation(Mathf.Max(0.01f, _lockShakeDuration),
          new Vector3(0f, 0f, _lockShakeAngle), 12, 0f, true)
        .SetUpdate(true).OnComplete(() => FinishReveal(true));
    }

    private void RestoreLock()
    {
      if (_hasLockRotation && _lock != null) _lock.transform.localRotation = _lockRotation;
      _hasLockRotation = false;
    }

    private void FinishReveal(bool playParticles)
    {
      _revealTween = null;
      RestoreLock();
      // FX is a sibling of Cover: hiding the cover leaves particles, frame and mask active.
      // Particle lifetime must not hold the cover or the input locked.
      if (playParticles && _openParticles != null) _openParticles.Play(true);
      if (_coverRoot != null) _coverRoot.SetActive(false);
    }

    private void OnDisable()
    {
      _revealTween?.Kill();
      _revealTween = null;
      RestoreLock();
      var tile = GetComponentInParent<MetaTile>();
      if (_paid || (tile != null && tile.IsCellOpen(this))) FinishReveal(false);
    }

    // Ray-plane intersection in sprite space: works for XY, XZ and rotated locations.
    // Six edges exclude the rectangular gaps between adjacent hexagons.
    public bool TryHit(Ray ray, out float distance)
    {
      distance = 0f;
      if (_cover == null || _cover.sprite == null || _coverRoot == null ||
          !_coverRoot.activeInHierarchy) return false;
      var t = _cover.transform;
      Vector3 origin = t.InverseTransformPoint(ray.origin);
      Vector3 direction = t.InverseTransformVector(ray.direction);
      if (Mathf.Abs(direction.z) < 0.000001f) return false;
      float travel = -origin.z / direction.z;
      if (travel < 0f) return false;
      var bounds = _cover.sprite.bounds;
      Vector3 point = origin + direction * travel - bounds.center;
      float x = Mathf.Abs(point.x) / Mathf.Max(bounds.extents.x, 0.000001f);
      float y = Mathf.Abs(point.y) / Mathf.Max(bounds.extents.y, 0.000001f);
      bool inside = _pointyTop ? x <= 1f && y <= 1f - 0.5f * x
                              : y <= 1f && x <= 1f - 0.5f * y;
      if (!inside) return false;
      distance = travel;
      return true;
    }

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
    public void EnsureUniqueId(HashSet<string> used)
    {
      if (!string.IsNullOrWhiteSpace(_cellId) && used.Add(_cellId)) return;
      UnityEditor.Undo.RecordObject(this, "Assign meta cell ID");
      _cellId = System.Guid.NewGuid().ToString("N");
      used.Add(_cellId);
      UnityEditor.EditorUtility.SetDirty(this);
      UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }

    public void ConfigureHitShape(bool pointyTop)
    {
      UnityEditor.Undo.RecordObject(this, "Configure hex input shape");
      _pointyTop = pointyTop;
      UnityEditor.EditorUtility.SetDirty(this);
      UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
    }

    public bool ValidateEffects(out string error)
    {
      error = _openParticles != null && _coverRoot != null &&
        _openParticles.transform.IsChildOf(_coverRoot.transform)
        ? "Open Particles must be outside Cover, otherwise hiding Cover stops the effect." : null;
      return error == null;
    }

    public void Configure(int cost, GameObject coverRoot, SpriteRenderer frame, SpriteMask mask,
      SpriteRenderer cover, SpriteRenderer lockRenderer, TextMeshPro priceLabel, ParticleSystem openParticles = null)
    {
      _hexCoinsCost = Mathf.Max(0, cost);
      _coverRoot = coverRoot;
      _frame = frame;
      _mask = mask;
      _cover = cover;
      _lock = lockRenderer;
      _priceLabel = priceLabel;
      _openParticles = openParticles;
      RefreshPrice();
    }
#endif
  }
}
