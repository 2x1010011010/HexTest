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

    [SerializeField, BoxGroup("FIREWORKS"),
     Tooltip("Preplaced effects on this window. Random selection; repeats between batches are allowed.")]
    private List<ParticleSystem> _fireworkPrefabs = new();
    [SerializeField, BoxGroup("FIREWORKS"), Min(1),
     Tooltip("Number of different preplaced effects launched together, limited by the list size.")]
    private int _simultaneousFireworks = 3;
    [SerializeField, BoxGroup("FIREWORKS"), Min(0f),
     Tooltip("Pause after all effects in the previous batch finish, in real-time seconds.")]
    private float _fireworksInterval = 0.2f;

    [SerializeField, BoxGroup("FIREWORKS"), Range(0f, 0.4f),
     Tooltip("Inset from each screen edge as a fraction of screen width/height.")]
    private float _fireworksScreenPadding = 0.12f;

    private Coroutine _fireworks;
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
      ClearFireworks();
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
          if (prefab != null && prefab.gameObject.scene.IsValid() && !available.Contains(prefab))
            available.Add(prefab);

      if (available.Count == 0)
      {
        _fireworks = null;
        yield break;
      }

      // Position only after the popup scale animation has settled.
      yield return new WaitForSeconds(Mathf.Max(0f, _openDuration));
      PrepareFireworksCanvas();
      Canvas.ForceUpdateCanvases();
      var activeEffects = new List<ParticleSystem>();
      while (true)
      {
        available.RemoveAll(prefab => prefab == null);
        if (available.Count == 0) break;

        activeEffects.Clear();
        var candidates = new List<ParticleSystem>(available);
        int count = Mathf.Min(Mathf.Max(1, _simultaneousFireworks), candidates.Count);
        for (int i = 0; i < count; i++)
        {
          int index = UnityEngine.Random.Range(0, candidates.Count);
          var effect = candidates[index];
          candidates.RemoveAt(index);
          effect.gameObject.SetActive(true);
          foreach (var particles in effect.GetComponentsInChildren<ParticleSystem>(true))
          {
            var main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.stopAction = ParticleSystemStopAction.None;
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

        PositionFireworks(activeEffects);
        // Each preplaced effect is launched at most once in this batch.
        foreach (var effect in activeEffects)
          if (effect != null && effect.gameObject.activeInHierarchy)
            effect.Play(true);
        do { yield return null; }
        while (HasAliveFireworks(activeEffects));
        ClearFireworks();
        // Always yield even when an asset removes itself and interval is zero.
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, _fireworksInterval));
      }
      StopFireworks();
    }

    private void PositionFireworks(List<ParticleSystem> effects)
    {
      if (effects.Count == 0) return;
      Canvas.ForceUpdateCanvases();
      Rect area = Screen.safeArea;
      Camera camera = _fireworksCanvas != null &&
                      _fireworksCanvas.renderMode == RenderMode.ScreenSpaceOverlay
        ? null : _fireworksCamera;
      if (camera != null)
      {
        Rect viewport = camera.pixelRect;
        area = Rect.MinMaxRect(Mathf.Max(area.xMin, viewport.xMin),
          Mathf.Max(area.yMin, viewport.yMin), Mathf.Min(area.xMax, viewport.xMax),
          Mathf.Min(area.yMax, viewport.yMax));
      }
      if (area.width <= 0f || area.height <= 0f) return;
      float padding = Mathf.Clamp(_fireworksScreenPadding, 0f, 0.4f);
      float insetX = area.width * padding;
      float insetY = area.height * padding;
      area = Rect.MinMaxRect(area.xMin + insetX, area.yMin + insetY,
        area.xMax - insetX, area.yMax - insetY);

      // Separate cells guarantee that a batch cannot pick the same launch point.
      int columns = Mathf.Clamp(Mathf.CeilToInt(Mathf.Sqrt(
        effects.Count * area.width / area.height)), 1, effects.Count);
      int rows = Mathf.CeilToInt((float)effects.Count / columns);
      var cells = new List<int>(columns * rows);
      for (int i = 0; i < columns * rows; i++) cells.Add(i);
      foreach (var effect in effects)
      {
        if (effect == null) continue;
        int index = UnityEngine.Random.Range(0, cells.Count);
        int cell = cells[index];
        cells.RemoveAt(index);
        Vector2 screenPoint = new Vector2(
          area.xMin + (cell % columns + UnityEngine.Random.Range(0.2f, 0.8f)) * area.width / columns,
          area.yMin + (cell / columns + UnityEngine.Random.Range(0.2f, 0.8f)) * area.height / rows);
        // Convert through the actual parent plane: CanvasScaler, anchors and pivot
        // need no manual pixel-to-UI scaling. The effect stays at local Z = 0.
        var parent = effect.transform.parent as RectTransform;
        if (parent != null && RectTransformUtility.ScreenPointToWorldPointInRectangle(
              parent, screenPoint, camera, out Vector3 worldPoint))
          effect.transform.position = worldPoint;
      }
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

    private void ClearFireworks()
    {
      if (_fireworkPrefabs == null) return;
      foreach (var effect in _fireworkPrefabs)
      {
        if (effect == null || !effect.gameObject.scene.IsValid()) continue;
        foreach (var particles in effect.GetComponentsInChildren<ParticleSystem>(true))
        {
          var main = particles.main;
          main.playOnAwake = false;
          main.stopAction = ParticleSystemStopAction.None;
          particles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
      }
    }

    private void StopFireworks()
    {
      if (_fireworks != null) StopCoroutine(_fireworks);
      _fireworks = null;
      ClearFireworks();
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
