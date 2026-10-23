using System.Globalization;
using TMPro;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Cells
{
  // Prefab view and authored values only. No score tracking or unlocking behavior.
  [DisallowMultipleComponent]
  [RequireComponent(typeof(SpriteRenderer))]
  public sealed class ScoreLocker : MonoBehaviour
  {
    [Header("References")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField, Tooltip("Reserved for the future unlock effect. Not played by this component.")]
    private ParticleSystem _unlockParticles;

    [Header("Authoring")]
    [SerializeField, Min(0)] private int _requiredScore;

    public SpriteRenderer SpriteRenderer => _spriteRenderer;
    public Canvas Canvas => _canvas;
    public TMP_Text ScoreText => _scoreText;
    public ParticleSystem UnlockParticles => _unlockParticles;
    public int RequiredScore => _requiredScore;

#if UNITY_EDITOR
    public void ConfigureForEditor(int requiredScore)
    {
      _requiredScore = Mathf.Max(0, requiredScore);
      RefreshEditorText();
    }

    public void AssignMissingEditorReferences()
    {
      if (_spriteRenderer == null)
        _spriteRenderer = GetComponent<SpriteRenderer>();
      if (_canvas == null)
        _canvas = GetComponentInChildren<Canvas>(true);
      if (_scoreText == null)
        _scoreText = _canvas != null
          ? _canvas.GetComponentInChildren<TMP_Text>(true)
          : GetComponentInChildren<TMP_Text>(true);
      if (_unlockParticles == null)
        _unlockParticles = GetComponentInChildren<ParticleSystem>(true);
    }

    public void RefreshEditorText()
    {
      if (_scoreText != null)
        _scoreText.text = _requiredScore.ToString(CultureInfo.InvariantCulture);
    }

    private void Reset()
    {
      AssignMissingEditorReferences();
      RefreshEditorText();
    }

    private void OnValidate()
    {
      // Do not mutate TMP objects from OnValidate: it may run during asset loading.
      _requiredScore = Mathf.Max(0, _requiredScore);
    }
#endif
  }
}
