using System;
using System.Collections.Generic;
using HexaSortTest.CodeBase.GameLogic.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace HexaSortTest.CodeBase.GameLogic.Meta
{
  public class MetaTile : MonoBehaviour
  {
    [SerializeField, BoxGroup("SETUP")] private string _tileId;
    [SerializeField, BoxGroup("SETUP")] private Material _blackMaterial;
    [SerializeField, BoxGroup("SETUP")] private Material _mainMaterial;

    [SerializeField, BoxGroup("GROUPS"),
     Tooltip("Reveal order. Group 0 is visible (in black material) from the start; " +
             "each group fully unlocking reveals the next.")]
    private List<MetaTileObjectGroup> _groups = new();

    [SerializeField, BoxGroup("PROGRESS SETTINGS"), Range(0.01f, 1f), FormerlySerializedAs("_progressPerTap")]
    private float _progressPerStep = 0.2f;

    [SerializeField, BoxGroup("PROGRESS SETTINGS"), FormerlySerializedAs("_currencyCostPerTap")]
    private int _hexCoinsCostPerStep = 10;

    private static readonly int ProgressId = Shader.PropertyToID("_Progress");
    private static readonly int MainColorId = Shader.PropertyToID("_MainColor");
    private static readonly int BlackColorId = Shader.PropertyToID("_BlackColor");
    private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    private static readonly int MinYId = Shader.PropertyToID("_MinY");
    private static readonly int MaxYId = Shader.PropertyToID("_MaxY");

    private int _currentGroupIndex;
    private int _currentObjectIndexInGroup;
    private float _currentObjectProgress;
    private bool _isTileFullyOpen;
    private MaterialPropertyBlock _propertyBlock;

    public event Action<MetaTile> OnProgressChanged;
    public event Action<MetaTile> OnObjectRevealed;
    public event Action<MetaTile> OnTileFullyOpened;

    public string TileId => _tileId;
    public bool IsTileFullyOpen => _isTileFullyOpen;
    public int GroupsCount => _groups?.Count ?? 0;
    public int HexCoinsCostPerStep => _hexCoinsCostPerStep;

    public int CurrentGroupIndex => _currentGroupIndex;
    public int CurrentObjectIndexInGroup => _currentObjectIndexInGroup;
    public float CurrentObjectProgress => _currentObjectProgress;

    private void Awake()
    {
      _propertyBlock = new MaterialPropertyBlock();
      HideAllGroups();
    }
    public void InitializeFromSave(MetaTileProgress saved)
    {
      int totalGroups = _groups.Count;

      _currentGroupIndex = Mathf.Clamp(saved?.UnlockedGroupsCount ?? 0, 0, totalGroups);
      _currentObjectIndexInGroup = Mathf.Max(0, saved?.UnlockedObjectsInGroupCount ?? 0);
      _currentObjectProgress = Mathf.Clamp01(saved?.CurrentObjectProgress ?? 0f);
      _isTileFullyOpen = totalGroups > 0 && _currentGroupIndex >= totalGroups;

      if (_isTileFullyOpen)
      {
        for (int g = 0; g < totalGroups; g++)
          RevealGroupInstantly(_groups[g]);
        return;
      }

      for (int g = 0; g < _currentGroupIndex; g++)
        RevealGroupInstantly(_groups[g]);

      if (_currentGroupIndex >= totalGroups)
        return;

      var currentGroup = _groups[_currentGroupIndex];
      SetGroupActive(currentGroup, true);

      for (int o = 0; o < currentGroup.Objects.Count; o++)
      {
        float progress = o < _currentObjectIndexInGroup ? 1f
          : o == _currentObjectIndexInGroup ? _currentObjectProgress
          : 0f;

        ApplyVisual(currentGroup.Objects[o], progress);
      }
    }
    
    public MetaTileStepResult AdvanceStep()
    {
      if (_isTileFullyOpen || _groups.Count == 0)
        return MetaTileStepResult.None;

      var group = _groups[_currentGroupIndex];

      if (group.Objects == null || group.Objects.Count == 0)
        return CompleteCurrentGroupAndAdvance();

      _currentObjectProgress += _progressPerStep;

      if (_currentObjectProgress >= 1f)
      {
        _currentObjectProgress = 0f;
        ApplyVisual(group.Objects[_currentObjectIndexInGroup], 1f);
        _currentObjectIndexInGroup++;

        if (_currentObjectIndexInGroup >= group.Objects.Count)
          return CompleteCurrentGroupAndAdvance();

        OnProgressChanged?.Invoke(this);
        OnObjectRevealed?.Invoke(this);
        return new MetaTileStepResult(true, true, false);
      }

      ApplyVisual(group.Objects[_currentObjectIndexInGroup], _currentObjectProgress);
      OnProgressChanged?.Invoke(this);
      return new MetaTileStepResult(true, false, false);
    }

    private MetaTileStepResult CompleteCurrentGroupAndAdvance()
    {
      _currentObjectIndexInGroup = 0;
      _currentObjectProgress = 0f;
      _currentGroupIndex++;

      bool tileFullyOpened = _currentGroupIndex >= _groups.Count;
      _isTileFullyOpen = tileFullyOpened;

      if (!tileFullyOpened)
      {
        var nextGroup = _groups[_currentGroupIndex];
        SetGroupActive(nextGroup, true);
        foreach (var obj in nextGroup.Objects)
          ApplyVisual(obj, 0f);
      }

      OnProgressChanged?.Invoke(this);
      OnObjectRevealed?.Invoke(this);
      if (tileFullyOpened)
        OnTileFullyOpened?.Invoke(this);

      return new MetaTileStepResult(true, true, tileFullyOpened);
    }

    private void HideAllGroups()
    {
      foreach (var group in _groups)
        SetGroupActive(group, false);
    }

    private void RevealGroupInstantly(MetaTileObjectGroup group)
    {
      SetGroupActive(group, true);
      foreach (var obj in group.Objects)
        ApplyVisual(obj, 1f);
    }

    private void SetGroupActive(MetaTileObjectGroup group, bool active)
    {
      if (group?.Objects == null)
        return;

      foreach (var obj in group.Objects)
        if (obj != null)
          obj.SetActive(active);
    }

    private void ApplyVisual(GameObject target, float progress)
    {
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
