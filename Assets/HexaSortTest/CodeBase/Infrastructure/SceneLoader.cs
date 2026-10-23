using System;
using System.Collections;
using HexaSortTest.CodeBase.Infrastructure.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HexaSortTest.CodeBase.Infrastructure
{
  public class SceneLoader
  {
    private readonly ICoroutineRunner _coroutineRunner;

    public SceneLoader(ICoroutineRunner coroutineRunner) =>
      _coroutineRunner = coroutineRunner;

    public void Load(string sceneName, Action onLoaded = null)
    {
#if UNITY_EDITOR
      GameLogger.Message($"[SceneLoader] Load requested: {SceneManager.GetActiveScene().name} -> {sceneName}.");
#endif
      _coroutineRunner.StartCoroutine(LoadScene(sceneName, onLoaded));
    }

    private IEnumerator LoadScene(string sceneName, Action onLoaded = null)
    {
      if (SceneManager.GetActiveScene().name == sceneName)
      {
#if UNITY_EDITOR
        GameLogger.Message($"[SceneLoader] Scene {sceneName} is already active; skipping load and invoking callback.");
#endif
        onLoaded?.Invoke();
#if UNITY_EDITOR
        GameLogger.Message($"[SceneLoader] Callback completed for active scene {sceneName}.");
#endif
        yield break;
      }

#if UNITY_EDITOR
      GameLogger.Message($"[SceneLoader] Starting async load of {sceneName}.");
#endif
      AsyncOperation waitNextScene;
      try
      {
        waitNextScene = SceneManager.LoadSceneAsync(sceneName);
      }
      catch (Exception exception)
      {
#if UNITY_EDITOR
        GameLogger.Error($"[SceneLoader] Failed to start loading {sceneName}: {exception}");
#endif
        throw;
      }
      while (waitNextScene is { isDone: false })
        yield return null;

#if UNITY_EDITOR
      GameLogger.Message($"[SceneLoader] Scene {sceneName} loaded; invoking callback.");
#endif
      onLoaded?.Invoke();
#if UNITY_EDITOR
      GameLogger.Message($"[SceneLoader] Callback completed for {sceneName}.");
#endif
    }
  }
}