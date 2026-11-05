using System.Collections.Generic;
using DG.Tweening;
using HexaSortTest.CodeBase.Infrastructure.Services.CurrencyService;
using TMPro;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.Meta
{
  public class MetaHexCoinsView : MonoBehaviour
  {
    [SerializeField] private TMP_Text _valueText;
    [SerializeField, Min(0.01f)] private float _duration = 0.35f;
    private readonly Queue<(int revision, int balance)> _changes = new();
    private ICurrencyService _currency;
    private Tween _tween;
    private int _displayed;
    private int _completedRevision;
    public int Revision { get; private set; }
    public Transform CoinSource => _valueText != null ? _valueText.transform : transform;
    public bool IsComplete(int revision) => !isActiveAndEnabled || _completedRevision >= revision;

    public void Configure(TMP_Text text) => _valueText = text;
    public void Bind(ICurrencyService currency)
    {
      if (_currency != null) _currency.OnHexCoinsChanged -= HandleChanged;
      _currency = currency;
      ResetView();
      if (isActiveAndEnabled && _currency != null) _currency.OnHexCoinsChanged += HandleChanged;
    }
    private void OnEnable()
    {
      ResetView();
      if (_currency != null) _currency.OnHexCoinsChanged += HandleChanged;
    }
    private void OnDisable()
    {
      if (_currency != null) _currency.OnHexCoinsChanged -= HandleChanged;
      ResetView();
    }
    public void ResetView()
    {
      _tween?.Kill();
      _tween = null;
      _changes.Clear();
      _completedRevision = Revision;
      SetValue(_currency?.HexCoins ?? 0);
    }
    private void SetValue(int value)
    {
      _displayed = value;
      if (_valueText != null) _valueText.text = value.ToString();
    }
    private void HandleChanged(int balance)
    {
      // Queue rapid purchases: each debit finishes before its own cell reveals.
      _changes.Enqueue((++Revision, balance));
      PlayNext();
    }
    private void PlayNext()
    {
      if (_tween != null || _changes.Count == 0) return;
      var change = _changes.Dequeue();
      _tween = DOTween.To(() => _displayed, SetValue, change.balance, Mathf.Max(0.01f, _duration))
        .SetEase(Ease.OutQuad).SetUpdate(true).OnComplete(() =>
        {
          _tween = null;
          _completedRevision = change.revision;
          PlayNext();
        });
    }
  }
}
