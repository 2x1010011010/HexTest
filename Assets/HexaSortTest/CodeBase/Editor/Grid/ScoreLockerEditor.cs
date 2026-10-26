using HexaSortTest.CodeBase.GameLogic.Cells;
using UnityEditor;
using UnityEngine;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  [CustomEditor(typeof(ScoreLocker)), CanEditMultipleObjects]
  public sealed class ScoreLockerEditor : UnityEditor.Editor
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
          Synchronize((ScoreLocker)item, true);

      if (GUILayout.Button("Assign Missing References"))
        foreach (var item in targets)
        {
          var locker = (ScoreLocker)item;
          Undo.RecordObject(locker, "Assign Score Locker References");
          locker.AssignMissingEditorReferences();
          Synchronize(locker, true);
        }

      EditorGUILayout.HelpBox(
        "Required Score is the total level score needed to open this lock, not a cost deducted from the score. The label and unlock logic use this same value.",
        MessageType.Info);
      foreach (var item in targets)
        DrawWarnings((ScoreLocker)item);
    }

    private static void Synchronize(ScoreLocker locker, bool recordUndo)
    {
      if (locker == null)
        return;
      if (locker.ScoreText != null && recordUndo)
        Undo.RecordObject(locker.ScoreText, "Update Score Locker Text");
      locker.RefreshEditorText();
      EditorUtility.SetDirty(locker);
      if (PrefabUtility.IsPartOfPrefabInstance(locker))
        PrefabUtility.RecordPrefabInstancePropertyModifications(locker);
      if (locker.ScoreText != null)
      {
        EditorUtility.SetDirty(locker.ScoreText);
        if (PrefabUtility.IsPartOfPrefabInstance(locker.ScoreText))
          PrefabUtility.RecordPrefabInstancePropertyModifications(locker.ScoreText);
      }
      SceneView.RepaintAll();
    }

    private static void DrawWarnings(ScoreLocker locker)
    {
      if (locker.SpriteRenderer == null || locker.Canvas == null || locker.ScoreText == null)
        EditorGUILayout.HelpBox($"{locker.name}: assign Sprite Renderer, Canvas and Score Text. Particle System can remain empty.", MessageType.Warning);
      if (locker.Canvas != null && locker.Canvas.renderMode != RenderMode.WorldSpace)
        EditorGUILayout.HelpBox($"{locker.name}: Canvas must use World Space.", MessageType.Warning);
    }

    private void OnUndoRedo()
    {
      foreach (var item in targets)
        Synchronize(item as ScoreLocker, false);
      Repaint();
    }
  }
}
