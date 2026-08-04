using System;
using UnityEditor;
using UnityEngine;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  public static class HexCellContextMenu
  {
    public static void Show(HexCellData cell, Action onStateChanged)
    {
      if (cell == null)
        return;

      var menu = new GenericMenu();

      AddOption(menu, "Enabled", cell, CellState.Enabled, onStateChanged);
      AddOption(menu, "Disabled", cell, CellState.Disabled, onStateChanged);
      AddOption(menu, "Spawn Point", cell, CellState.SpawnPoint, onStateChanged);

      menu.ShowAsContext();
    }

    private static void AddOption(GenericMenu menu, string label, HexCellData cell, CellState state,
      Action onStateChanged)
    {
      menu.AddItem(new GUIContent(label), cell.State == state, () =>
      {
        cell.State = state;
        onStateChanged?.Invoke();
      });
    }
  }
}