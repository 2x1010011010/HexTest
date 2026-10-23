using System;
using HexaSortTest.CodeBase.GameLogic.Cells;
using UnityEditor;
using UnityEngine;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  public static class HexCellContextMenu
  {
    public static void Show(HexCellData cell, HexGridSettings settings, Action onStateChanged)
    {
      if (cell == null)
        return;

      Vector2 mouse = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
      var menu = new GenericMenu();
      AddState(menu, "Enabled", cell, CellState.Enabled, onStateChanged);
      AddState(menu, "Disabled", cell, CellState.Disabled, onStateChanged);
      AddState(menu, "Spawn Point", cell, CellState.SpawnPoint, onStateChanged);
      menu.AddSeparator("");
      AddType(menu, "Hex Type/Normal", cell, CellCoverType.Normal, settings, onStateChanged);
      AddType(menu, "Hex Type/Score Lock", cell, CellCoverType.ScoreLock, settings, onStateChanged);
      AddType(menu, "Hex Type/Breakable Blocker", cell, CellCoverType.BreakableBlocker, settings, onStateChanged);
      if (cell.CoverType == CellCoverType.ScoreLock)
        menu.AddItem(new GUIContent($"Unlock Score... ({cell.RequiredScore})"), false,
          () => ValuePopup.Show("Required Score", cell.RequiredScore, 0, value =>
          {
            cell.RequiredScore = value;
            onStateChanged?.Invoke();
          }, mouse));
      if (cell.CoverType == CellCoverType.BreakableBlocker)
        menu.AddItem(new GUIContent($"Required Hits... ({cell.RequiredHits})"), false,
          () => ValuePopup.Show("Required Hits", cell.RequiredHits, 1, value =>
          {
            cell.RequiredHits = value;
            onStateChanged?.Invoke();
          }, mouse));
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
      HexGridSettings settings, Action changed)
    {
      menu.AddItem(new GUIContent(label), cell.CoverType == type, () =>
      {
        if (type == CellCoverType.ScoreLock && cell.CoverType != CellCoverType.ScoreLock)
          cell.RequiredScore = Mathf.Max(0, settings.DefaultRequiredScore);
        if (type == CellCoverType.BreakableBlocker && cell.CoverType != CellCoverType.BreakableBlocker)
          cell.RequiredHits = Mathf.Max(1, settings.DefaultRequiredHits);
        cell.CoverType = type;
        if (type != CellCoverType.Normal)
          cell.State = CellState.Enabled;
        changed?.Invoke();
      });
    }

    private sealed class ValuePopup : EditorWindow
    {
      private string _label;
      private int _value;
      private int _minimum;
      private Action<int> _apply;

      public static void Show(string label, int value, int minimum, Action<int> apply, Vector2 mouse)
      {
        var popup = CreateInstance<ValuePopup>();
        popup._label = label;
        popup._value = Mathf.Max(minimum, value);
        popup._minimum = minimum;
        popup._apply = apply;
        popup.position = new Rect(mouse, new Vector2(260, 80));
        popup.ShowPopup();
        popup.Focus();
      }

      private void OnGUI()
      {
        if (_apply == null)
        {
          Close();
          return;
        }
        _value = Mathf.Max(_minimum, EditorGUILayout.IntField(_label, _value));
        if (GUILayout.Button("Apply"))
        {
          _apply(_value);
          Close();
        }
      }

      private void OnLostFocus() => Close();
    }
  }
}
