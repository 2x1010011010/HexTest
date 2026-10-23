using UnityEngine;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  // Shared authoring path: preview and saved prefabs have the same cover geometry.
  internal static class HexCellCoverBuilder
  {
    public static SpriteRenderer Create(HexCellData cell, HexGridSettings settings, Transform parent)
    {
      Sprite sprite = settings.CoverSprite(cell.CoverType);
      if (sprite == null || cell.State == CellState.Disabled)
        return null;

      Vector2 footprint = settings.CellSize;
      Bounds bounds = settings.TryGetModelBounds(out Bounds modelBounds)
        ? modelBounds
        : new Bounds(settings.ModelOffset, new Vector3(footprint.x, 0f, footprint.y));
      float scale = Mathf.Min(footprint.x / Mathf.Max(0.0001f, sprite.bounds.size.x),
        footprint.y / Mathf.Max(0.0001f, sprite.bounds.size.y)) * Mathf.Max(0.0001f, settings.CoverScaleFor(cell.CoverType));
      float elevation = Mathf.Min(footprint.x, footprint.y) * Mathf.Max(0f, settings.CoverElevationFor(cell.CoverType));

      var go = new GameObject($"{cell.CoverType}_Cover");
      // Sibling of the model: avoids distortion from its arbitrary rotation/non-uniform scale.
      go.transform.SetParent(parent, false);
      go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
      go.transform.localScale = Vector3.one * scale;
      Vector3 center = cell.Position + new Vector3(bounds.center.x, bounds.max.y + elevation, bounds.center.z);
      go.transform.localPosition = center - go.transform.localRotation * (sprite.bounds.center * scale);

      var renderer = go.AddComponent<SpriteRenderer>();
      renderer.sprite = sprite;
      return renderer;
    }
  }
}
