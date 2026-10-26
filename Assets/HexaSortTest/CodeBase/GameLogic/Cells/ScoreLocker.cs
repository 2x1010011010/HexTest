using System.Globalization;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Cells
{
  [DisallowMultipleComponent]
  [RequireComponent(typeof(SpriteRenderer))]
  public sealed class ScoreLocker : MonoBehaviour
  {
    [SerializeField, BoxGroup("REFERENCES")] private SpriteRenderer _spriteRenderer;
    [SerializeField, BoxGroup("REFERENCES")] private Canvas _canvas;
    [SerializeField, BoxGroup("REFERENCES")] private TMP_Text _scoreText;
    [SerializeField, BoxGroup("REFERENCES")] private ParticleSystem _unlockParticles;
    
    [SerializeField, Min(0), BoxGroup("AUTHORING")] private int _requiredScore;

    public SpriteRenderer SpriteRenderer => _spriteRenderer;
    public Canvas Canvas => _canvas;
    public TMP_Text ScoreText => _scoreText;
    public ParticleSystem UnlockParticles => _unlockParticles;
    public int RequiredScore => _requiredScore;

    public void ConfigureRequiredScore(int requiredScore)
    {
      _requiredScore = Mathf.Max(0, requiredScore);
      SetRemainingScore(_requiredScore);
    }

    public void SetRemainingScore(int remaining)
    {
      if (_scoreText != null)
        _scoreText.text = Mathf.Max(0, remaining).ToString(CultureInfo.InvariantCulture);
    }

#if UNITY_EDITOR
    public void ConfigureForEditor(int requiredScore)
    {
      ConfigureRequiredScore(requiredScore);
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
      _requiredScore = Mathf.Max(0, _requiredScore);
    }
#endif
  }
}
