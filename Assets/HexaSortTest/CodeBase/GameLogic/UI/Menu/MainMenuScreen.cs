using System;
using System.Collections;
using HexaSortTest.CodeBase.Infrastructure.Services.CurrencyService;
using HexaSortTest.CodeBase.Infrastructure.Services.LivesService;
using TMPro;
using Zenject;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.Menu
{
  public class MainMenuScreen : MonoBehaviour
  {
    [SerializeField, BoxGroup("BUTTONS")] private MainMenuPlayButton _playButton;

    [SerializeField, BoxGroup("COUNTERS")] private TMP_Text _livesText;
    [SerializeField, BoxGroup("COUNTERS")] private TMP_Text _lifeTimerText;
    [SerializeField, BoxGroup("COUNTERS")] private TMP_Text _coinsText;
    [SerializeField, BoxGroup("COUNTERS"), Tooltip("Optional existing timer container; kept visible for Full and countdown.")]
    private GameObject _lifeTimerRoot;

    private ICurrencyService _currency;
    private LivesService _lives;
    private Coroutine _timerRoutine;
    private bool _subscribed;

    [Inject]
    public void Construct(ICurrencyService currency, LivesService lives)
    {
      StopCounters();
      _currency = currency;
      _lives = lives;
      if (isActiveAndEnabled) StartCounters();
    }

    public event Action OnPlayClicked;

    private void OnEnable()
    {
      StartCounters();
      if (_playButton == null)
      {
        Debug.LogError("[MainMenuScreen] _playButton is not assigned in the inspector!");
      }
      else
      {
        _playButton.OnPlayButtonClick += HandlePlayClicked;
      }
    }

    private void OnDisable()
    {
      StopCounters();
      if (_playButton != null)
        _playButton.OnPlayButtonClick -= HandlePlayClicked;
    }

    private void StartCounters()
    {
      if (_subscribed || _currency == null || _lives == null) return;
      _subscribed = true;
      _currency.OnCoinsChanged += UpdateCoins;
      _lives.OnLivesChanged += UpdateLives;
      RefreshCounters();
      _timerRoutine = StartCoroutine(UpdateTimer());
    }

    private void StopCounters()
    {
      if (_timerRoutine != null) StopCoroutine(_timerRoutine);
      _timerRoutine = null;
      if (!_subscribed) return;
      _currency.OnCoinsChanged -= UpdateCoins;
      _lives.OnLivesChanged -= UpdateLives;
      _subscribed = false;
    }

    private IEnumerator UpdateTimer()
    {
      var delay = new WaitForSecondsRealtime(0.25f);
      while (true)
      {
        yield return delay;
        _lives.Refresh();
        UpdateLifeTimer();
      }
    }

    private void OnApplicationFocus(bool focused)
    {
      if (focused && _subscribed) RefreshCounters();
    }

    private void OnApplicationPause(bool paused)
    {
      if (!paused && _subscribed) RefreshCounters();
    }

    private void RefreshCounters()
    {
      _lives.Refresh();
      UpdateCoins(_currency.Coins);
      UpdateLives(_lives.Lives);
    }

    private void UpdateCoins(int value)
    {
      if (_coinsText != null) _coinsText.text = value.ToString();
    }

    private void UpdateLives(int value)
    {
      if (_livesText != null) _livesText.text = value.ToString();
      UpdateLifeTimer();
    }

    private void UpdateLifeTimer()
    {
      // Keep the old optional container reference compatible with existing prefabs.
      if (_lifeTimerRoot != null && !_lifeTimerRoot.activeSelf)
        _lifeTimerRoot.SetActive(true);
      if (_lifeTimerText == null) return;
      _lifeTimerText.enabled = true;
      if (_lives.Lives >= _lives.MaxLives)
      {
        _lifeTimerText.text = "Full";
        return;
      }

      int seconds = (int)Math.Ceiling(_lives.TimeToNextLife.TotalSeconds);
      _lifeTimerText.text = $"{seconds / 60:00}:{seconds % 60:00}";
    }

    private void HandlePlayClicked()
    {
      Debug.Log("[MainMenuScreen] _playClicked");
      OnPlayClicked?.Invoke();
    }
  }
}
