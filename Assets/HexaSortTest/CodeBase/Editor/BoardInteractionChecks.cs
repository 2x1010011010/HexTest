using System;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using UnityEditor;
using UnityEngine;

namespace HexaSortTest.CodeBase.Editor
{
  public static class BoardInteractionChecks
  {
    [MenuItem("Tools/Tests/Run Board State And Rotation Checks")]
    public static void RunAll()
    {
      BoardSessionChecks.Run();
      Debug.Log("[BoardInteractionChecks] PASS: state ownership, scoring, results, cancellation and 60-degree snapping.");
    }
  }
}
