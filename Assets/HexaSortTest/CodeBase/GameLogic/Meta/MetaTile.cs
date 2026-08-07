using System;
using System.Collections.Generic;
using HexaSortTest.CodeBase.GameLogic.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Meta
{
  public class MetaTile : MonoBehaviour
  {
    [SerializeField, BoxGroup("SETUP")] private string _tileId;
    [SerializeField, BoxGroup("SETUP")] private Material _blackMaterial;
    [SerializeField, BoxGroup("SETUP")] private Material _mainMaterial;
    [SerializeField, BoxGroup("SETUP")] private List<GameObject> _objectsOnTile = new();
    [SerializeField, BoxGroup("PROGRESS SETTINGS"), Range(0.01f, 1f)] private float _progressPerTap = 0.2f;
    [SerializeField, BoxGroup("PROGRESS SETTINGS")] private int _currencyCostPerTap = 10;

    private static readonly int ProgressId = Shader.PropertyToID("_Progress");
    private static readonly int MainColorId = Shader.PropertyToID("_MainColor");
    private static readonly int BlackColorId = Shader.PropertyToID("_BlackColor");
    private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    private static readonly int MinYId = Shader.PropertyToID("_MinY");
    private static readonly int MaxYId = Shader.PropertyToID("_MaxY");

    private int _currentObjectIndex;
    private bool _isTileFullyOpen;
    private float _progressCounter;
    private MaterialPropertyBlock _propertyBlock;

    public event Action<MetaTile> OnProgressChanged;
    public event Action<MetaTile> OnTileFullyOpened;

    public string TileId => _tileId;
    public bool IsTileOpen => _isTileFullyOpen;
    public int CurrentObjectIndex => _currentObjectIndex;
    public float CurrentObjectProgress => _progressCounter;
    public int CurrencyCostPerTap => _currencyCostPerTap;

    private void Awake() =>
      _propertyBlock = new MaterialPropertyBlock();

    public void InitializeFromSave(MetaTileProgress saved)
    {
      _currentObjectIndex = saved?.UnlockedObjectsCount ?? 0;
      _progressCounter = saved?.CurrentObjectProgress ?? 0f;
      _isTileFullyOpen = _objectsOnTile.Count > 0 && _currentObjectIndex >= _objectsOnTile.Count;

      for (int i = 0; i < _objectsOnTile.Count; i++)
      {
        float progress = i < _currentObjectIndex ? 1f
          : i == _currentObjectIndex ? _progressCounter
          : 0f;

        ApplyVisual(i, progress);
      }
    }

    public bool Open()
    {
      if (_isTileFullyOpen)
        return false;

      if (_objectsOnTile == null || _objectsOnTile.Count == 0)
      {
        _isTileFullyOpen = true;
        OnTileFullyOpened?.Invoke(this);
        return true;
      }

      _progressCounter += _progressPerTap;
      bool objectCompleted = false;

      if (_progressCounter >= 1f)
      {
        _progressCounter = 0f;
        ApplyVisual(_currentObjectIndex, 1f);
        _currentObjectIndex++;
        objectCompleted = true;

        if (_currentObjectIndex >= _objectsOnTile.Count)
        {
          _isTileFullyOpen = true;
          OnProgressChanged?.Invoke(this);
          OnTileFullyOpened?.Invoke(this);
          return true;
        }
      }

      ApplyVisual(_currentObjectIndex, _progressCounter);
      OnProgressChanged?.Invoke(this);
      return objectCompleted;
    }

    private void ApplyVisual(int objectIndex, float progress)
    {
      if (_objectsOnTile == null || objectIndex < 0 || objectIndex >= _objectsOnTile.Count)
        return;

      var target = _objectsOnTile[objectIndex];
      if (target == null)
        return;

      foreach (var rend in target.GetComponentsInChildren<Renderer>())
        ApplyVisualToRenderer(rend, progress);
    }

    private void ApplyVisualToRenderer(Renderer rend, float progress)
    {
      rend.GetPropertyBlock(_propertyBlock);

      Bounds localBounds = GetLocalBounds(rend);
      _propertyBlock.SetFloat(MinYId, localBounds.min.y);
      _propertyBlock.SetFloat(MaxYId, localBounds.max.y);
      _propertyBlock.SetFloat(ProgressId, progress);

      if (_mainMaterial != null)
      {
        if (_mainMaterial.HasProperty(MainColorId))
          _propertyBlock.SetColor(MainColorId, _mainMaterial.color);
        if (_mainMaterial.mainTexture != null)
          _propertyBlock.SetTexture(MainTexId, _mainMaterial.mainTexture);
      }

      if (_blackMaterial != null && _blackMaterial.HasProperty(BlackColorId))
        _propertyBlock.SetColor(BlackColorId, _blackMaterial.color);

      rend.SetPropertyBlock(_propertyBlock);
    }

    private Bounds GetLocalBounds(Renderer rend)
    {
      if (rend.TryGetComponent(out MeshFilter meshFilter) && meshFilter.sharedMesh != null)
        return meshFilter.sharedMesh.bounds;

      if (rend.TryGetComponent(out SkinnedMeshRenderer skinned) && skinned.sharedMesh != null)
        return skinned.sharedMesh.bounds;

      return new Bounds(Vector3.zero, Vector3.one);
    }
  }
}