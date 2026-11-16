using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.ResultPopup
{
  public class GameResultPopup : UIWindow
  {
    [SerializeField, BoxGroup("VIEWS")] private GameObject _victoryView;
    [SerializeField, BoxGroup("VIEWS")] private GameObject _defeatView;

    [SerializeField, BoxGroup("BUTTONS")] private GameResultContinueButton _continueButton;
    [SerializeField, BoxGroup("BUTTONS")] private GameResultMainMenuButton _mainMenuButton;

    [SerializeField, BoxGroup("ANIMATION")]
    private float _openDuration = 0.3f;

    [SerializeField, BoxGroup("REWARDS")] private TMP_Text _victoryCoinsText;
    [SerializeField, BoxGroup("REWARDS")] private TMP_Text _victoryHexCoinsText;
    [SerializeField, BoxGroup("REWARDS")] private TMP_Text _defeatCoinsText;
    [SerializeField, BoxGroup("REWARDS")] private TMP_Text _defeatHexCoinsText;

    [SerializeField, BoxGroup("FIREWORKS"), AssetsOnly,
     Tooltip("Random prefab on every launch. Repeats are allowed.")]
    private List<ParticleSystem> _fireworkPrefabs = new();
    [SerializeField, BoxGroup("FIREWORKS"),
     Tooltip("Optional world-space origin. If empty, effects appear at the screen centre with their prefab offsets.")]
    private Transform _fireworksOrigin;
    [SerializeField, BoxGroup("FIREWORKS"), Min(1),
     Tooltip("Number of effects launched together in each batch. Repeats are allowed.")]
    private int _simultaneousFireworks = 3;
    [SerializeField, BoxGroup("FIREWORKS"), Min(0f),
     Tooltip("Pause after all effects in the previous batch finish, in real-time seconds.")]
    private float _fireworksInterval = 0.2f;

    private Coroutine _fireworks;
    private GameObject _activeFireworkRoot;
    private Canvas _fireworksCanvas;
    private Camera _fireworksCamera;
    private Camera _previousCanvasCamera;
    private float _previousPlaneDistance;
    private bool _restoreOverlayCanvas;

    public void SetRewards(int coins, int hexCoins)
    {
      if (_victoryCoinsText != null) _victoryCoinsText.text = $"+{coins}";
      if (_victoryHexCoinsText != null) _victoryHexCoinsText.text = $"+{hexCoins}";
      if (_defeatCoinsText != null) _defeatCoinsText.text = $"+{coins}";
      if (_defeatHexCoinsText != null) _defeatHexCoinsText.text = $"+{hexCoins}";
    }

    public event Action OnContinueClicked;
    public event Action OnMainMenuClicked;

    private void OnEnable()
    {
      if (_continueButton == null)
        Debug.LogError("[GameResultPopup] _continueButton is not assigned in the inspector!");
      else
        _continueButton.OnContinueButtonClick += HandleContinueClicked;

      if (_mainMenuButton == null)
        Debug.LogError("[GameResultPopup] _mainMenuButton is not assigned in the inspector!");
      else
        _mainMenuButton.OnMainMenuButtonClick += HandleMainMenuClicked;
    }

    private void OnDisable()
    {
      StopFireworks();
      if (_continueButton != null)
        _continueButton.OnContinueButtonClick -= HandleContinueClicked;

      if (_mainMenuButton != null)
        _mainMenuButton.OnMainMenuButtonClick -= HandleMainMenuClicked;
    }

    public void ShowVictory()
    {
      Debug.Log(
        $"[GameResultPopup] ShowVictory on {gameObject.name}. victoryView={(_victoryView != null)}, defeatView={(_defeatView != null)}");

      if (_victoryView != null) _victoryView.SetActive(true);
      if (_defeatView != null) _defeatView.SetActive(false);
      Open();
      StartFireworks();
    }

    public void ShowDefeat()
    {
      StopFireworks();
      Debug.Log(
        $"[GameResultPopup] ShowDefeat on {gameObject.name}. victoryView={(_victoryView != null)}, defeatView={(_defeatView != null)}");

      if (_victoryView != null) _victoryView.SetActive(false);
      if (_defeatView != null) _defeatView.SetActive(true);
      Open();
    }

    public override void Open()
    {
      Debug.Log($"[GameResultPopup] Open called on {gameObject.name}. activeBefore={gameObject.activeSelf}");
      base.Open();
      transform.localScale = Vector3.zero;
      transform.DOScale(Vector3.one, _openDuration).SetEase(Ease.OutBack);
    }

    public override void Close()
    {
      StopFireworks();
      base.Close();
    }

    private void StartFireworks()
    {
      StopFireworks();
      if (isActiveAndEnabled) _fireworks = StartCoroutine(PlayFireworks());
    }

    private IEnumerator PlayFireworks()
    {
      // StartCoroutine returns before any early completion can clear its handle.
      yield return null;
      var available = new List<ParticleSystem>();
      if (_fireworkPrefabs != null)
        foreach (var prefab in _fireworkPrefabs)
          if (prefab != null) available.Add(prefab);

      if (available.Count == 0)
      {
        _fireworks = null;
        yield break;
      }

      PrepareFireworksCanvas();
      var activeEffects = new List<ParticleSystem>();
      while (true)
      {
        available.RemoveAll(prefab => prefab == null);
        if (available.Count == 0) break;

        // Configure under an inactive parent, before Play On Awake can run.
        _activeFireworkRoot = new GameObject("Victory Fireworks");
        _activeFireworkRoot.SetActive(false);
        var root = _activeFireworkRoot.transform;
        if (_fireworksOrigin != null)
          root.SetParent(_fireworksOrigin, false);
        else if (_fireworksCamera != null)
        {
          float distance = _fireworksCanvas != null &&
                           _fireworksCanvas.renderMode == RenderMode.ScreenSpaceCamera
            ? _fireworksCanvas.planeDistance
            : Mathf.Min(10f, _fireworksCamera.farClipPlane * 0.5f);
          distance = Mathf.Max(_fireworksCamera.nearClipPlane + 0.01f, distance - 0.1f);
          root.position = _fireworksCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, distance));
          root.rotation = _fireworksCamera.transform.rotation;
        }
        else
          root.SetParent(transform, false);

        activeEffects.Clear();
        int count = Mathf.Max(1, _simultaneousFireworks);
        for (int i = 0; i < count; i++)
        {
          var prefab = available[UnityEngine.Random.Range(0, available.Count)];
          var effect = Instantiate(prefab, root, false);
          effect.gameObject.SetActive(true);
          foreach (var particles in effect.GetComponentsInChildren<ParticleSystem>(true))
          {
            var main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.useUnscaledTime = true;
            particles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
          }
          if (_fireworksCanvas != null)
            foreach (var renderer in effect.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
              renderer.sortingLayerID = _fireworksCanvas.sortingLayerID;
              renderer.sortingOrder = Mathf.Max(_fireworksCanvas.sortingOrder + 1, renderer.sortingOrder);
            }
          activeEffects.Add(effect);
        }

        _activeFireworkRoot.SetActive(true);
        // Launch the entire batch in this frame. Asset scripts may remove instances.
        foreach (var effect in activeEffects)
          if (effect != null && effect.gameObject.activeInHierarchy)
            effect.Play(true);
        do { yield return null; }
        while (HasAliveFireworks(activeEffects));
        ClearActiveFirework();
        // Always yield even when an asset removes itself and interval is zero.
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, _fireworksInterval));
      }
      StopFireworks();
    }

    private static bool HasAliveFireworks(List<ParticleSystem> effects)
    {
      foreach (var effect in effects)
        if (effect != null && effect.gameObject.activeInHierarchy && effect.IsAlive(true))
          return true;
      return false;
    }

    private void PrepareFireworksCanvas()
    {
      var canvas = _victoryView != null ? _victoryView.GetComponentInParent<Canvas>() : null;
      if (canvas == null) canvas = GetComponentInChildren<Canvas>(true);
      _fireworksCanvas = canvas != null ? canvas.rootCanvas : null;
      _fireworksCamera = _fireworksCanvas != null && _fireworksCanvas.worldCamera != null
        ? _fireworksCanvas.worldCamera : Camera.main;

      // ParticleSystemRenderer cannot render over Screen Space Overlay.
      // Change only the live instance and restore its original settings on exit.
      if (_fireworksCanvas == null || _fireworksCamera == null ||
          _fireworksCanvas.renderMode != RenderMode.ScreenSpaceOverlay) return;
      _previousCanvasCamera = _fireworksCanvas.worldCamera;
      _previousPlaneDistance = _fireworksCanvas.planeDistance;
      _restoreOverlayCanvas = true;
      _fireworksCanvas.renderMode = RenderMode.ScreenSpaceCamera;
      _fireworksCanvas.worldCamera = _fireworksCamera;
      _fireworksCanvas.planeDistance = Mathf.Clamp(10f,
        _fireworksCamera.nearClipPlane + 0.02f, _fireworksCamera.farClipPlane - 0.02f);
    }

    private void ClearActiveFirework()
    {
      if (_activeFireworkRoot == null) return;
      _activeFireworkRoot.SetActive(false);
      Destroy(_activeFireworkRoot);
      _activeFireworkRoot = null;
    }

    private void StopFireworks()
    {
      if (_fireworks != null) StopCoroutine(_fireworks);
      _fireworks = null;
      ClearActiveFirework();
      if (_restoreOverlayCanvas && _fireworksCanvas != null)
      {
        _fireworksCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _fireworksCanvas.worldCamera = _previousCanvasCamera;
        _fireworksCanvas.planeDistance = _previousPlaneDistance;
      }
      _restoreOverlayCanvas = false;
      _fireworksCanvas = null;
      _fireworksCamera = null;
      _previousCanvasCamera = null;
    }

    private void HandleContinueClicked()
    {
      Debug.Log("[GameResultPopup] HandleContinueClicked");
      StopFireworks();
      OnContinueClicked?.Invoke();
    }

    private void HandleMainMenuClicked()
    {
      Debug.Log("[GameResultPopup] HandleMainMenuClicked");
      StopFireworks();
      OnMainMenuClicked?.Invoke();
    }
  }
}
