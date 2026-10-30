#if UNITY_EDITOR
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HexaSortTest.CodeBase.GameLogic.Cells;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HexaSortTest.CodeBase.Editor
{
  public static class NonBlockingVfxChecks
  {
    [MenuItem("Tools/HexaSort/Checks/Non-blocking VFX (Play Mode)")]
    public static void Run()
    {
      if (!Application.isPlaying)
        throw new InvalidOperationException("Run this check in Play Mode.");
      CheckLock(false);
      CheckLock(true);
      Debug.Log("NonBlockingVfxChecks passed: cell unlocks immediately while a 60-second effect keeps playing, with particles on the cover root or its child.");
    }

    private static void CheckLock(bool particlesOnRoot)
    {
      var cell = new GameObject("VFX regression check");
      cell.SetActive(false);
      try
      {
        cell.AddComponent<Cell>();
        var cover = cell.AddComponent<CellCoverData>();
        var visual = new GameObject("Lock");
        visual.transform.SetParent(cell.transform, false);
        var locker = visual.AddComponent<ScoreLocker>();
        var effect = particlesOnRoot ? visual : new GameObject("Particles");
        if (!particlesOnRoot) effect.transform.SetParent(visual.transform, false);
        var particles = effect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 60f;
        main.startLifetime = 60f;
        var serialized = new SerializedObject(locker);
        serialized.FindProperty("_unlockParticles").objectReferenceValue = particles;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        cover.Initialize(CellCoverType.ScoreLock, 10, 0, visual);
        cell.SetActive(true);

        var pending = cover.UpdateScoreAsync(10, CancellationToken.None);
        Require(pending.Status == UniTaskStatus.Succeeded, "Unlock must complete in the same frame");
        pending.GetAwaiter().GetResult();
        Require(!cover.IsBlocked && !cover.IsUnlocking, "Cell must already be unlocked");
        Require(particles.gameObject.activeInHierarchy && particles.isPlaying, "VFX must survive hiding the lock");
        Require(!visual.GetComponent<SpriteRenderer>().isVisible || !visual.activeInHierarchy ||
                !visual.GetComponent<SpriteRenderer>().enabled, "Lock sprite must be hidden");
        if (LayerMask.NameToLayer("Grid") >= 0)
          Require(cell.layer == LayerMask.NameToLayer("Grid"), "Grid layer must change immediately");
        cover.UpdateScoreAsync(20, CancellationToken.None).GetAwaiter().GetResult();
        Require(!cover.IsBlocked, "Repeated score updates must not relock the cell");
      }
      finally { Object.DestroyImmediate(cell); }
    }

    private static void Require(bool condition, string message)
    {
      if (!condition) throw new Exception("NonBlockingVfxChecks: " + message);
    }
  }
}
#endif
