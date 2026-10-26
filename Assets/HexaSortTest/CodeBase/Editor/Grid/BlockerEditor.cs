using HexaSortTest.CodeBase.GameLogic.Cells;
using UnityEditor;
using UnityEngine;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  [CustomEditor(typeof(Blocker)), CanEditMultipleObjects]
  public sealed class BlockerEditor : UnityEditor.Editor
  {
    private void OnEnable() => Undo.undoRedoPerformed += OnUndoRedo;
    private void OnDisable() => Undo.undoRedoPerformed -= OnUndoRedo;

    public override void OnInspectorGUI()
    {
      serializedObject.Update();
      using (new EditorGUI.DisabledScope(true))
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
      DrawPropertiesExcluding(serializedObject, "m_Script");
      if (serializedObject.ApplyModifiedProperties())
        foreach (var item in targets)
          Synchronize((Blocker)item, true);

      if (GUILayout.Button("Assign Missing References"))
        foreach (var item in targets)
        {
          var blocker = (Blocker)item;
          Undo.RecordObject(blocker, "Assign Blocker References");
          blocker.AssignMissingEditorReferences();
          Synchronize(blocker, true);
        }

      EditorGUILayout.HelpBox("Sprite = Sprite List[Required Hits - 1]. The grid editor sets each cell's Required Hits when previewing or saving. Hit Particles plays on each adjacent collection. The cell unlocks after the final effect.", MessageType.Info);
      foreach (var item in targets)
      {
        var blocker = (Blocker)item;
        if (blocker.SpriteRenderer == null)
          EditorGUILayout.HelpBox($"{blocker.name}: assign Sprite Renderer.", MessageType.Warning);
        if (!blocker.TryGetSprite(blocker.RequiredHits, out _))
          EditorGUILayout.HelpBox($"{blocker.name}: assign Sprite List element {blocker.RequiredHits - 1} for Required Hits = {blocker.RequiredHits}. No sprite will be shown until this entry is assigned.", MessageType.Warning);
      }
    }

    private static void Synchronize(Blocker blocker, bool recordUndo)
    {
      if (blocker == null)
        return;
      if (blocker.SpriteRenderer != null && recordUndo)
        Undo.RecordObject(blocker.SpriteRenderer, "Update Blocker Sprite");
      blocker.RefreshEditorSprite();
      EditorUtility.SetDirty(blocker);
      if (PrefabUtility.IsPartOfPrefabInstance(blocker))
        PrefabUtility.RecordPrefabInstancePropertyModifications(blocker);
      if (blocker.SpriteRenderer != null)
      {
        EditorUtility.SetDirty(blocker.SpriteRenderer);
        if (PrefabUtility.IsPartOfPrefabInstance(blocker.SpriteRenderer))
          PrefabUtility.RecordPrefabInstancePropertyModifications(blocker.SpriteRenderer);
      }
      SceneView.RepaintAll();
    }

    private void OnUndoRedo()
    {
      foreach (var item in targets)
        Synchronize(item as Blocker, false);
      Repaint();
    }
  }
}
