using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace HexaSortTest.CodeBase.Editor.Grid
{
  // Preview scenes do not reliably submit CanvasRenderer geometry. Render copies
  // of the generated TMP meshes as ordinary geometry, retaining the prefab layout.
  internal sealed class HexGridPreviewText
  {
    private readonly List<Mesh> _meshes = new();
    private readonly HashSet<TextMeshProUGUI> _baked = new();
    private readonly Dictionary<Material, Material> _materials = new();

    public void Build(GameObject previewRoot)
    {
      foreach (var text in previewRoot.GetComponentsInChildren<TextMeshProUGUI>())
      {
        if (!text.isActiveAndEnabled || text.font == null)
          continue;

        text.ForceMeshUpdate(false, true);
        TMP_TextInfo info = text.textInfo;
        Canvas canvas = text.canvas;
        bool hasProxy = false;
        for (int i = 0; i < info.materialCount; i++)
        {
          TMP_MeshInfo source = info.meshInfo[i];
          if (source.mesh == null || source.vertexCount == 0)
            continue;
          Material sourceMaterial = source.material != null ? source.material : text.fontSharedMaterial;
          if (sourceMaterial == null)
            continue;

          Mesh mesh = Object.Instantiate(source.mesh);
          mesh.name = "Preview TMP Mesh";
          mesh.hideFlags = HideFlags.HideAndDontSave;
          mesh.RecalculateBounds();
          _meshes.Add(mesh);

          var proxy = new GameObject("Preview TMP Geometry") { hideFlags = HideFlags.HideAndDontSave };
          proxy.layer = text.gameObject.layer;
          proxy.transform.SetParent(text.transform, false);
          proxy.AddComponent<MeshFilter>().sharedMesh = mesh;
          var renderer = proxy.AddComponent<MeshRenderer>();
          renderer.sharedMaterial = PreviewMaterial(sourceMaterial);
          renderer.shadowCastingMode = ShadowCastingMode.Off;
          renderer.receiveShadows = false;
          renderer.lightProbeUsage = LightProbeUsage.Off;
          renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
          if (canvas != null)
          {
            renderer.sortingLayerID = canvas.sortingLayerID;
            renderer.sortingOrder = canvas.sortingOrder;
          }
          hasProxy = true;
        }

        // Only this temporary copy is disabled, after all meshes/materials were
        // copied. Prevent duplicate drawing if the native UI path also renders.
        if (hasProxy)
        {
          _baked.Add(text);
          text.enabled = false;
        }
      }
    }

    public bool WasBaked(TMP_Text text) => text is TextMeshProUGUI ui && _baked.Contains(ui);

    private Material PreviewMaterial(Material source)
    {
      if (_materials.TryGetValue(source, out Material material))
        return material;
      material = new Material(source)
      {
        name = "Preview TMP Material",
        hideFlags = HideFlags.HideAndDontSave
      };
      // The uploaded TMP SDF shader uses this UI depth-test uniform. Canvas would
      // normally supply it; a regular MeshRenderer needs an explicit value.
      material.SetInt("unity_GUIZTestMode", (int)CompareFunction.Always);
      _materials.Add(source, material);
      return material;
    }

    public void Clear()
    {
      _baked.Clear();
      foreach (var mesh in _meshes)
        if (mesh != null)
          Object.DestroyImmediate(mesh);
      _meshes.Clear();
      foreach (var material in _materials.Values)
        if (material != null)
          Object.DestroyImmediate(material);
      _materials.Clear();
    }
  }
}
