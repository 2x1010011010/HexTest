using System;
using System.Collections.Generic;
using System.Threading;
using HexaSortTest.CodeBase.GameLogic.Cells;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HexaSortTest.CodeBase.Editor
{
  public static class ScoreLockChecks
  {
    [MenuItem("Tools/HexaSort/Checks/Score locks 30-30-60-60")]
    public static void Run()
    {
      if (Application.isPlaying)
        throw new InvalidOperationException("Run this check outside Play Mode.");
      var root = new GameObject("ScoreLockChecks") { hideFlags = HideFlags.HideAndDontSave };
      root.SetActive(false);
      try
      {
        var covers = new List<CellCoverData>();
        int[] thresholds = { 60, 30, 30, 60 }; // Deliberately not sorted by price.
        int[] staleThresholds = { 10, 40, 50, 90 };
        for (int i = 0; i < thresholds.Length; i++)
        {
          var cell = new GameObject("Cell_" + i);
          cell.transform.SetParent(root.transform);
          cell.AddComponent<Cell>();
          var cover = cell.AddComponent<CellCoverData>();
          var visual = new GameObject("Lock_" + i);
          visual.transform.SetParent(root.transform);
          var locker = visual.AddComponent<ScoreLocker>();
          cover.Initialize(CellCoverType.ScoreLock, thresholds[i], 0, visual);
          // Reproduce an older cell whose duplicate threshold disagrees with its lock.
          var serialized = new SerializedObject(cover);
          serialized.FindProperty("_requiredScore").intValue = staleThresholds[i];
          serialized.ApplyModifiedPropertiesWithoutUndo();
          Require(locker.RequiredScore == thresholds[i], "Authored lock threshold changed");
          covers.Add(cover);
        }
        foreach (int score in new[] { 0, 10, 29, 30, 40, 59, 60, 90 })
        {
          for (int i = 0; i < covers.Count; i++)
          {
            var cover = covers[i];
            cover.UpdateScoreAsync(score, CancellationToken.None).GetAwaiter().GetResult();
            bool blocked = score < thresholds[i];
            Require(cover.IsBlocked == blocked, $"Lock {i} at score {score}");
            Require(cover.VisualRoot.activeSelf == blocked, $"Visual {i} at score {score}");
            Require(cover.RequiredScore == thresholds[i], "Score update mutated the authored threshold");
            if (!blocked && LayerMask.NameToLayer("Grid") >= 0)
              Require(cover.gameObject.layer == LayerMask.NameToLayer("Grid"), "Unlocked cell layer");
          }
        }
        Debug.Log("ScoreLockChecks passed: 30/30 unlock together at 30; 60/60 at 60, regardless of stale cell thresholds or iteration order.");
      }
      finally { Object.DestroyImmediate(root); }
    }

    [MenuItem("Tools/HexaSort/Checks/Audit selected grid locks")]
    public static void AuditSelected()
    {
      if (Selection.activeGameObject == null)
        throw new InvalidOperationException("Select the level grid in Hierarchy or Prefab Mode.");
      var owners = new Dictionary<GameObject, CellCoverData>();
      foreach (var cover in Selection.activeGameObject.GetComponentsInChildren<CellCoverData>(true))
      {
        if (cover.Type != CellCoverType.ScoreLock) continue;
        var visual = cover.VisualRoot;
        if (visual == null)
        {
          Debug.LogError($"Score lock {cover.name}: Visual Root is missing.", cover);
          continue;
        }
        if (owners.TryGetValue(visual, out var other))
          Debug.LogError($"Two cells reference the same lock: {other.name}, {cover.name}. Assign a separate Visual Root to each cell.", cover);
        else owners.Add(visual, cover);
        var locker = visual.GetComponentInChildren<ScoreLocker>(true);
        int fallback = new SerializedObject(cover).FindProperty("_requiredScore").intValue;
        Debug.Log($"Cell {cover.name} at {cover.transform.position}: visual={visual.name}, " +
          $"effective threshold={cover.RequiredScore}, legacy cell threshold={fallback}.", cover);
        if (locker == null)
          Debug.LogWarning("Visual Root has no ScoreLocker; the legacy cell threshold is used.", cover);
        else if (locker.ScoreText != null && !locker.ScoreText.transform.IsChildOf(visual.transform))
          Debug.LogError("Score Text references an object outside this lock's visual. Assign its own TMP label.", locker);
      }
    }

    private static void Require(bool condition, string message)
    {
      if (!condition) throw new Exception("Score lock check failed: " + message);
    }
  }
}
