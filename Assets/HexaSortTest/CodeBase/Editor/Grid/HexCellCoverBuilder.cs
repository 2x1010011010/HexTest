using System.Globalization;
using HexaSortTest.CodeBase.GameLogic.Cells;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  // Authoring only: instantiate supplied visuals and write initial level values.
  internal static class HexCellCoverBuilder
  {
    public static GameObject Create(HexCellData cell, HexGridSettings settings, Transform parent, bool preview = false)
    {
      if (cell.State == CellState.Disabled)
        return null;

      GameObject prefab = settings.CoverPrefab(cell.CoverType);
      if (prefab != null)
        return CreateFromPrefab(cell, settings, parent, prefab, preview);

      // Preserve old sprite-based levels while visual prefabs are being authored.
      Sprite sprite = settings.CoverSprite(cell.CoverType);
      if (sprite == null)
        return null;

      Vector2 footprint = settings.CellSize;
      Bounds bounds = ModelBounds(settings);
      float scale = Mathf.Min(footprint.x / Mathf.Max(0.0001f, sprite.bounds.size.x),
        footprint.y / Mathf.Max(0.0001f, sprite.bounds.size.y)) * Mathf.Max(0.0001f, settings.CoverScaleFor(cell.CoverType));
      var go = new GameObject($"{cell.CoverType}_Cover");
      go.transform.SetParent(parent, false);
      go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
      go.transform.localScale = Vector3.one * scale;
      go.transform.localPosition = CoverCenter(cell, settings, bounds)
        - go.transform.localRotation * (sprite.bounds.center * scale);
      go.AddComponent<SpriteRenderer>().sprite = sprite;
      return go;
    }

    public static TMP_Text FindScoreText(GameObject root, string path)
    {
      if (root == null)
        return null;
      var locker = root.GetComponent<ScoreLocker>();
      if (locker != null)
        return locker.ScoreText;
      if (string.IsNullOrWhiteSpace(path))
        return root.GetComponentInChildren<TMP_Text>(true);
      Transform target = root.transform.Find(path);
      return target != null ? target.GetComponent<TMP_Text>() : null;
    }

    private static GameObject CreateFromPrefab(HexCellData cell, HexGridSettings settings,
      Transform parent, GameObject prefab, bool preview)
    {
      var go = preview
        ? Object.Instantiate(prefab, parent, false)
        : (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = prefab.transform.localRotation;
      go.transform.localScale = prefab.transform.localScale;

      if (cell.CoverType == CellCoverType.ScoreLock)
      {
        var locker = go.GetComponent<ScoreLocker>();
        if (locker != null)
        {
          locker.ConfigureForEditor(cell.RequiredScore);
          if (!preview)
            PrefabUtility.RecordPrefabInstancePropertyModifications(locker);
        }
        TMP_Text text = FindScoreText(go, settings.LockScoreTextPath);
        if (text != null)
        {
          // Compatibility for previously authored TMP-only visuals.
          if (locker == null)
            text.text = cell.RequiredScore.ToString(CultureInfo.InvariantCulture);
          text.ForceMeshUpdate(true, true);
          if (!preview)
            PrefabUtility.RecordPrefabInstancePropertyModifications(text);
        }
      }

      if (cell.CoverType == CellCoverType.BreakableBlocker)
      {
        var blocker = go.GetComponent<Blocker>();
        if (blocker != null)
        {
          blocker.ConfigureForEditor(cell.RequiredHits);
          if (!preview)
          {
            PrefabUtility.RecordPrefabInstancePropertyModifications(blocker);
            if (blocker.SpriteRenderer != null)
              PrefabUtility.RecordPrefabInstancePropertyModifications(blocker.SpriteRenderer);
          }
        }
      }

      // Effects are not played while editing, and no stage/damage scripts are invoked.
      if (preview)
        foreach (var particles in go.GetComponentsInChildren<ParticleSystem>(true))
          particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

      Bounds artwork = ArtworkBounds(go);
      Vector2 footprint = settings.CellSize;
      float factor = Mathf.Min(footprint.x / Mathf.Max(0.0001f, artwork.size.x),
        footprint.y / Mathf.Max(0.0001f, artwork.size.y))
        * Mathf.Max(0.0001f, settings.CoverScaleFor(cell.CoverType));
      Quaternion planeRotation = Quaternion.Euler(90f, 0f, 0f);
      go.transform.localRotation = planeRotation * prefab.transform.localRotation;
      go.transform.localScale = prefab.transform.localScale * factor;
      go.transform.localPosition = CoverCenter(cell, settings, ModelBounds(settings))
        - planeRotation * (artwork.center * factor);
      if (!preview)
        PrefabUtility.RecordPrefabInstancePropertyModifications(go.transform);
      return go;
    }

    private static Bounds ArtworkBounds(GameObject root)
    {
      // Visible sprite artwork defines fitting; TMP labels and particle effects must not resize the cover.
      bool found = false;
      Bounds bounds = new Bounds();
      foreach (var renderer in root.GetComponentsInChildren<SpriteRenderer>())
      {
        if (!renderer.enabled)
          continue;
        if (!found)
        {
          bounds = renderer.bounds;
          found = true;
        }
        else
          bounds.Encapsulate(renderer.bounds);
      }
      if (found)
        return bounds;

      // UI-only prefabs may use a World Space Canvas with an Image and TMP label.
      RectTransform rect = root.GetComponent<RectTransform>();
      if (rect == null)
      {
        Canvas canvas = root.GetComponentInChildren<Canvas>();
        if (canvas != null)
          rect = canvas.GetComponent<RectTransform>();
      }
      if (rect != null)
      {
        var corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        bounds = new Bounds(corners[0], Vector3.zero);
        for (int i = 1; i < corners.Length; i++)
          bounds.Encapsulate(corners[i]);
        return bounds;
      }
      foreach (var renderer in root.GetComponentsInChildren<Renderer>())
      {
        if (!renderer.enabled || renderer is ParticleSystemRenderer || renderer is TrailRenderer
          || renderer.GetComponent<TMP_Text>() != null)
          continue;
        if (!found)
        {
          bounds = renderer.bounds;
          found = true;
        }
        else
          bounds.Encapsulate(renderer.bounds);
      }
      return found ? bounds : new Bounds(Vector3.zero, Vector3.one);
    }

    private static Bounds ModelBounds(HexGridSettings settings)
    {
      Vector2 size = settings.CellSize;
      return settings.TryGetModelBounds(out Bounds bounds)
        ? bounds : new Bounds(settings.ModelOffset, new Vector3(size.x, 0f, size.y));
    }

    private static Vector3 CoverCenter(HexCellData cell, HexGridSettings settings, Bounds modelBounds)
    {
      Vector2 size = settings.CellSize;
      float elevation = Mathf.Min(size.x, size.y) * Mathf.Max(0f, settings.CoverElevationFor(cell.CoverType));
      return cell.Position + new Vector3(modelBounds.center.x, modelBounds.max.y + elevation, modelBounds.center.z);
    }
  }
}
