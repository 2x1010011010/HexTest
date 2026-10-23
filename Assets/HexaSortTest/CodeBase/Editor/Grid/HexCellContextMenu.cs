using System;
using HexaSortTest.CodeBase.GameLogic.Cells;
using UnityEditor;
using UnityEngine;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  public static class HexCellContextMenu
  {
    public static void Show(HexCellData cell, int defaultRequiredScore, Action onStateChanged)
    {
      if (cell == null)
        return;

      Vector2 mouse = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
      var menu = new GenericMenu();
      AddState(menu, "Enabled", cell, CellState.Enabled, onStateChanged);
      AddState(menu, "Disabled", cell, CellState.Disabled, onStateChanged);
      AddState(menu, "Spawn Point", cell, CellState.SpawnPoint, onStateChanged);
      menu.AddSeparator("");
      AddType(menu, "Hex Type/Normal", cell, CellCoverType.Normal, defaultRequiredScore, onStateChanged);
      AddType(menu, "Hex Type/Score Lock", cell, CellCoverType.ScoreLock, defaultRequiredScore, onStateChanged);
      AddType(menu, "Hex Type/Breakable Blocker", cell, CellCoverType.BreakableBlocker, defaultRequiredScore, onStateChanged);
      if (cell.CoverType == CellCoverType.ScoreLock)
        menu.AddItem(new GUIContent($"Unlock Score... ({cell.RequiredScore})"), false,
          () => ScorePopup.Show(cell, onStateChanged, mouse));
      menu.ShowAsContext();
    }

    private static void AddState(GenericMenu menu, string label, HexCellData cell, CellState state, Action changed)
    {
      menu.AddItem(new GUIContent(label), cell.State == state, () =>
      {
        cell.State = state;
        if (state == CellState.SpawnPoint)
          cell.CoverType = CellCoverType.Normal;
        changed?.Invoke();
      });
    }

    private static void AddType(GenericMenu menu, string label, HexCellData cell, CellCoverType type,
      int defaultRequiredScore, Action changed)
    {
      menu.AddItem(new GUIContent(label), cell.CoverType == type, () =>
      {
        if (type == CellCoverType.ScoreLock && cell.CoverType != CellCoverType.ScoreLock)
          cell.RequiredScore = Mathf.Max(0, defaultRequiredScore);
        cell.CoverType = type;
        if (type != CellCoverType.Normal)
          cell.State = CellState.Enabled;
        changed?.Invoke();
      });
    }

    private sealed class ScorePopup : EditorWindow
    {
      private HexCellData _cell;
      private Action _changed;
      private int _score;

      public static void Show(HexCellData cell, Action changed, Vector2 mouse)
      {
        var popup = CreateInstance<ScorePopup>();
        popup._cell = cell;
        popup._changed = changed;
        popup._score = cell.RequiredScore;
        popup.position = new Rect(mouse, new Vector2(260, 80));
        popup.ShowPopup();
        popup.Focus();
      }

      private void OnGUI()
      {
        if (_cell == null)
        {
          Close();
          return;
        }
        _score = Mathf.Max(0, EditorGUILayout.IntField("Required Score", _score));
        if (GUILayout.Button("Apply"))
        {
          _cell.RequiredScore = _score;
          _changed?.Invoke();
          Close();
        }
      }

      private void OnLostFocus() => Close();
    }
  }
}
