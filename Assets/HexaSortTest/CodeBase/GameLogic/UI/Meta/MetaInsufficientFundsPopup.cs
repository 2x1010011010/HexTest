using System;
using HexaSortTest.CodeBase.Infrastructure.Services.LivesService;
using TMPro;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.Meta
{
  public class MetaInsufficientFundsPopup : MonoBehaviour
  {
    [SerializeField] private TMP_Text _message;
    [SerializeField] private MetaPopupCloseButton _closeButton;
    [SerializeField] private MetaPopupPlayButton _playButton;
    private LivesService _lives;
    private Action _play;
    private bool _startingLevel;
    private float _nextRefresh;
    public bool IsOpen => gameObject.activeInHierarchy;
    private void OnEnable()
    {
      if (_closeButton != null) _closeButton.OnCloseButtonClick += Close;
      if (_playButton != null) _playButton.OnPlayButtonClick += Play;
      RefreshPlayButton();
    }

    private void OnDisable()
    {
      if (_closeButton != null) _closeButton.OnCloseButtonClick -= Close;
      if (_playButton != null) _playButton.OnPlayButtonClick -= Play;
    }

    public void Bind(LivesService lives, Action play)
    {
      _lives = lives;
      _play = play;
      RefreshPlayButton();
    }

    public void Configure(TMP_Text message, MetaPopupCloseButton closeButton, MetaPopupPlayButton playButton)
    {
      Close();
      _message = message;
      _closeButton = closeButton;
      _playButton = playButton;
    }

    private void Update()
    {
      if (Time.unscaledTime < _nextRefresh) return;
      _nextRefresh = Time.unscaledTime + 0.25f;
      RefreshPlayButton();
    }

    private void RefreshPlayButton()
    {
      _lives?.Refresh();
      if (_playButton != null && _playButton.Button != null)
        _playButton.Button.interactable = !_startingLevel && _play != null &&
          _lives != null && _lives.Lives > 0;
    }

    private void Play()
    {
      // Recheck at click time; refreshing also restores lives earned while in meta.
      RefreshPlayButton();
      if (_startingLevel || _play == null || _lives == null || _lives.Lives <= 0) return;
      _startingLevel = true;
      Close();
      _play.Invoke();
    }
    public void Show(int cost, int balance)
    {
      _startingLevel = false;
      RefreshPlayButton();
      if (_message != null)
        _message.text = $"Not enough HexCoins\n\nRequired: {cost}\nBalance: {balance}\nMissing: {Mathf.Max(0, cost - balance)}";
      gameObject.SetActive(true);
      transform.SetAsLastSibling();
    }
    public void Close() => gameObject.SetActive(false);
  }
}
