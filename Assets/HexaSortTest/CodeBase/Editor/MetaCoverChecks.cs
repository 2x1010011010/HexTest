using System;
using System.Collections.Generic;
using System.Reflection;
using HexaSortTest.CodeBase.GameLogic.Data;
using HexaSortTest.CodeBase.GameLogic.Meta;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HexaSortTest.CodeBase.Editor
{
  public static class MetaCoverChecks
  {
    [MenuItem("Tools/Tests/Run Meta Cover Checks")]
    public static void Run()
    {
      if (EditorApplication.isPlayingOrWillChangePlaymode)
        throw new InvalidOperationException("Run outside Play Mode.");
      var root = new GameObject("MetaCoverChecks") { hideFlags = HideFlags.HideAndDontSave };
      try
      {
        var tile = root.AddComponent<MetaTile>();
        var picture = Child(root, "Picture");
        var a = Child(root, "Cover A");
        var b = Child(root, "Cover B");
        tile.ConfigureAuthoring("test", 10, new List<GameObject> { a, b });
        Require(a.activeSelf && b.activeSelf && picture.activeSelf, "All covers initially closed");
        Require(tile.AdvanceStep(false).ObjectRevealed && !a.activeSelf && b.activeSelf, "One step removes exactly one cover");
        var save = new MetaTileProgress("test")
        {
          UnlockedGroupsCount = tile.CurrentGroupIndex,
          UnlockedObjectsInGroupCount = tile.CurrentObjectIndexInGroup,
          CurrentObjectProgress = tile.CurrentObjectProgress
        };
        tile.InitializeFromSave(null);
        tile.InitializeFromSave(save);
        Require(!a.activeSelf && b.activeSelf, "Reload restores removed covers");
        Require(tile.AdvanceStep(false).TileFullyOpened && !b.activeSelf && picture.activeSelf, "Completion leaves picture visible");
        Require(!tile.AdvanceStep(false).Success, "No paid steps after completion");
        tile.InitializeFromSave(new MetaTileProgress("other") { UnlockedGroupsCount = 99 });
        Require(a.activeSelf && b.activeSelf, "Other location save ignored");
        tile.InitializeFromSave(new MetaTileProgress("test") { UnlockedObjectsInGroupCount = 999 });
        Require(tile.IsTileFullyOpen && !a.activeSelf && !b.activeSelf, "Stale cursor safely normalized");
        tile.ConfigureAuthoring("test", 10, new List<GameObject> { a, a });
        Require(!tile.AdvanceStep(false).Success, "Duplicate layout cannot charge");
        tile.ConfigureAuthoring("test", 10, new List<GameObject> { a, null });
        Require(!tile.AdvanceStep(false).Success, "Missing cover cannot charge");

        var first = new MetaTileObjectGroup(); first.Objects.Add(a);
        var second = new MetaTileObjectGroup(); second.Objects.Add(b);
        Set(tile, "_groups", new List<MetaTileObjectGroup> { first, second });
        Set(tile, "_progressPerStep", 0.25f);
        tile.InitializeFromSave(null);
        tile.AdvanceStep(false);
        Require(a.activeSelf && b.activeSelf && Mathf.Approximately(tile.CurrentObjectProgress, 0.25f), "Partial legacy progress retains both covers");
        tile.AdvanceStep(false); tile.AdvanceStep(false); tile.AdvanceStep(false);
        Require(!a.activeSelf && b.activeSelf && tile.CurrentGroupIndex == 1, "Future group remains covered");
        tile.InitializeFromSave(null);
        Require(a.activeSelf && b.activeSelf, "Rollback restores both covers");
        Debug.Log("[MetaCoverChecks] PASS. Run Progress Save Checks too for currency/save transactions.");
      }
      finally { Object.DestroyImmediate(root); }
    }

    private static GameObject Child(GameObject root, string name)
    {
      var child = new GameObject(name);
      child.transform.SetParent(root.transform, false);
      return child;
    }
    private static void Set(object target, string name, object value) =>
      target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
    private static void Require(bool condition, string message)
    {
      if (!condition) throw new Exception("[MetaCoverChecks] FAIL: " + message);
    }
  }
}
