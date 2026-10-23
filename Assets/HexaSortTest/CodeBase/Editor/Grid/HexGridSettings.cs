using System;
using HexaSortTest.CodeBase.GameLogic.Cells;
using UnityEngine;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  [Serializable]
  public class HexGridSettings
  {
    public GridType GridType = GridType.Rectangular;
    public int Width = 3;
    public int Height = 5;
    public int Radius = 3;

    public GameObject HexPrefab;
    public bool AutomaticModelSize = true;
    public Vector2 ModelSize = Vector2.one;
    public Vector3 ModelScale = Vector3.one;
    public Vector3 ModelRotation;
    public Vector3 ModelOffset;
    public bool FlatTop;
    public float Spacing = 1f;

    [SerializeField] private int _coverSettingsVersion;

    public Sprite LockSprite;
    public Sprite BlockerSprite;
    public int DefaultRequiredScore = 100;
    public float LockScale = 1f;
    public float LockElevation = 0.02f;
    public float BlockerScale = 1f;
    public float BlockerElevation = 0.02f;

    // Keep the original JSON field names for migration of existing editor settings.
    [SerializeField] private float CoverScale = 1f;
    [SerializeField] private float CoverElevation = 0.02f;

    public void EnsureCoverDefaults()
    {
      if (_coverSettingsVersion < 1)
      {
        DefaultRequiredScore = 100;
        CoverScale = 1f;
        CoverElevation = 0.02f;
      }
      if (_coverSettingsVersion < 2)
      {
        LockScale = BlockerScale = Mathf.Max(0.0001f, CoverScale);
        LockElevation = BlockerElevation = Mathf.Max(0f, CoverElevation);
        _coverSettingsVersion = 2;
      }
    }

    public float CoverScaleFor(CellCoverType type) => type switch
    {
      CellCoverType.ScoreLock => LockScale,
      CellCoverType.BreakableBlocker => BlockerScale,
      _ => 1f
    };

    public float CoverElevationFor(CellCoverType type) => type switch
    {
      CellCoverType.ScoreLock => LockElevation,
      CellCoverType.BreakableBlocker => BlockerElevation,
      _ => 0f
    };

    public Sprite CoverSprite(CellCoverType type) => type switch
    {
      CellCoverType.ScoreLock => LockSprite,
      CellCoverType.BreakableBlocker => BlockerSprite,
      _ => null
    };

    public Quaternion Rotation => HexPrefab.transform.localRotation * Quaternion.Euler(ModelRotation);
    public Vector3 Scale => Vector3.Scale(HexPrefab.transform.localScale, ModelScale);

    public Vector2 CellSize
    {
      get
      {
        Vector2 size = ModelSize;
        if (AutomaticModelSize && TryGetModelBounds(out Bounds bounds))
          size = new Vector2(bounds.size.x, bounds.size.z);
        return new Vector2(Mathf.Max(0.0001f, size.x), Mathf.Max(0.0001f, size.y));
      }
    }

    // Measure renderer bounds in the final grid axes, including child transforms.
    public bool TryGetModelBounds(out Bounds bounds)
    {
      bounds = new Bounds();
      if (HexPrefab == null)
        return false;

      bool found = false;
      Matrix4x4 modelMatrix = Matrix4x4.TRS(ModelOffset, Rotation, Scale);
      foreach (Renderer renderer in HexPrefab.GetComponentsInChildren<Renderer>())
      {
        if (!renderer.enabled)
          continue;

        Bounds localBounds = renderer.localBounds;
        Matrix4x4 matrix = modelMatrix * HexPrefab.transform.worldToLocalMatrix * renderer.transform.localToWorldMatrix;
        for (int x = -1; x <= 1; x += 2)
          for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
            {
              Vector3 corner = localBounds.center + Vector3.Scale(localBounds.extents, new Vector3(x, y, z));
              Vector3 point = matrix.MultiplyPoint3x4(corner);
              if (!found)
              {
                bounds = new Bounds(point, Vector3.zero);
                found = true;
              }
              else
                bounds.Encapsulate(point);
            }
      }
      return found;
    }

    public void ApplyModelTransform(Transform target, Vector3 cellPosition)
    {
      target.localPosition = cellPosition + ModelOffset;
      target.localRotation = Rotation;
      target.localScale = Scale;
    }
  }
}
