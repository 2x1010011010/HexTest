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
    [SerializeField, BoxGroup("GROUPS"), Tooltip("Cover objects in reveal order. Keep the background outside these objects.")]
    private List<MetaTileObjectGroup> _groups = new();
    [SerializeField, BoxGroup("PROGRESS SETTINGS"), Range(0.01f, 1f), FormerlySerializedAs("_progressPerTap")]
    private float _progressPerStep = 1f;
    [SerializeField, BoxGroup("PROGRESS SETTINGS"), Min(0), FormerlySerializedAs("_currencyCostPerTap")]
    private int _hexCoinsCostPerStep = 10;

    private int _currentGroupIndex;
    private int _currentObjectIndexInGroup;
    private float _currentObjectProgress;
    private bool _isTileFullyOpen;

    public event Action<MetaTile> OnProgressChanged;
    public event Action<MetaTile> OnObjectRevealed;
    public event Action<MetaTile> OnTileFullyOpened;
    public string TileId => _tileId;
    public bool IsTileFullyOpen => _isTileFullyOpen;
    public int GroupsCount => _groups?.Count ?? 0;
    public int HexCoinsCostPerStep
    {
      get
      {
        var cell = CurrentCell;
        return cell != null ? cell.HexCoinsCost : _hexCoinsCostPerStep;
      }
    }
    private MetaLocationCell CurrentCell
    {
      get
      {
        if (_currentGroupIndex < 0 || _currentGroupIndex >= GroupsCount) return null;
        var objects = _groups[_currentGroupIndex]?.Objects;
        if (objects == null || _currentObjectIndexInGroup < 0 || _currentObjectIndexInGroup >= objects.Count) return null;
        var cover = objects[_currentObjectIndexInGroup];
        return cover != null ? cover.GetComponentInParent<MetaLocationCell>() : null;
      }
    }
    public int CurrentGroupIndex => _currentGroupIndex;
    public int CurrentObjectIndexInGroup => _currentObjectIndexInGroup;
    public float CurrentObjectProgress => _currentObjectProgress;

    private void Awake() => InitializeFromSave(null);

    public void InitializeFromSave(MetaTileProgress saved)
    {
      // A save from a different location must never reveal this location.
      if (saved != null && saved.TileId != _tileId) saved = null;
      _currentGroupIndex = Mathf.Clamp(saved?.UnlockedGroupsCount ?? 0, 0, GroupsCount);
      _currentObjectIndexInGroup = Mathf.Max(0, saved?.UnlockedObjectsInGroupCount ?? 0);
      float progress = saved?.CurrentObjectProgress ?? 0f;
      _currentObjectProgress = float.IsNaN(progress) || float.IsInfinity(progress) ? 0f : Mathf.Clamp01(progress);
      NormalizeCursor();
      RefreshVisuals();
    }

    public MetaTileStepResult AdvanceStep(bool notify = true)
    {
      // Reject malformed layouts before the observer spends currency.
      if (!ValidateLayout(out _)) return MetaTileStepResult.None;
      NormalizeCursor();
      if (_isTileFullyOpen || GroupsCount == 0) return MetaTileStepResult.None;

      _currentObjectProgress = Mathf.Clamp01(_currentObjectProgress + (CurrentCell != null ? 1f : _progressPerStep));
      bool revealed = _currentObjectProgress >= 1f - 0.00001f;
      if (revealed)
      {
        _currentObjectProgress = 0f;
        _currentObjectIndexInGroup++;
        NormalizeCursor();
      }
      RefreshVisuals();
      var result = new MetaTileStepResult(true, revealed, _isTileFullyOpen);
      if (notify) NotifyStepCompleted(result);
      return result;
    }

    private void NormalizeCursor()
    {
      // Empty groups and stale indices never cost a paid step or index outside a list.
      while (_currentGroupIndex < GroupsCount)
      {
        int count = _groups[_currentGroupIndex]?.Objects?.Count ?? 0;
        if (_currentObjectIndexInGroup < count) break;
        _currentGroupIndex++;
        _currentObjectIndexInGroup = 0;
        _currentObjectProgress = 0f;
      }
      _isTileFullyOpen = GroupsCount > 0 && _currentGroupIndex >= GroupsCount;
      if (_isTileFullyOpen)
      {
        _currentObjectIndexInGroup = 0;
        _currentObjectProgress = 0f;
      }
    }

    private void RefreshVisuals()
    {
      for (int g = 0; g < GroupsCount; g++)
      {
        var objects = _groups[g]?.Objects;
        if (objects == null) continue;
        for (int o = 0; o < objects.Count; o++)
        {
          var cover = objects[o];
          if (cover == null || cover == gameObject || !cover.transform.IsChildOf(transform)) continue;
          bool opened = g < _currentGroupIndex || (g == _currentGroupIndex && o < _currentObjectIndexInGroup);
          // All future covers remain opaque. Do not reveal future groups early.
          cover.SetActive(!opened);
        }
      }
    }

    public void NotifyStepCompleted(MetaTileStepResult result)
    {
      if (!result.Success) return;
      OnProgressChanged?.Invoke(this);
      if (result.ObjectRevealed) OnObjectRevealed?.Invoke(this);
      if (result.TileFullyOpened) OnTileFullyOpened?.Invoke(this);
    }

    public bool ValidateLayout(out string error)
    {
      if (string.IsNullOrWhiteSpace(_tileId)) { error = "Tile Id is empty."; return false; }
      if (_hexCoinsCostPerStep < 0 || float.IsNaN(_progressPerStep) ||
          _progressPerStep <= 0f || _progressPerStep > 1f)
      { error = "Invalid cost or progress per step."; return false; }
      var seen = new HashSet<GameObject>();
      for (int g = 0; g < GroupsCount; g++)
      {
        var objects = _groups[g]?.Objects;
        if (objects == null || objects.Count == 0)
        { error = $"Group {g} is empty. Collect covers again."; return false; }
        foreach (var cover in objects)
        {
          var cell = cover != null ? cover.GetComponentInParent<MetaLocationCell>() : null;
          if (cell != null && (cell.HexCoinsCost < 0 || cell.CoverRoot != cover))
          { error = $"Group {g}: invalid cell cost or cover root."; return false; }
          if (cover == null || cover == gameObject || !cover.transform.IsChildOf(transform) || !seen.Add(cover))
          { error = $"Group {g}: missing, duplicate, or non-child cover."; return false; }
        }
      }
      foreach (var cover in seen)
        for (var parent = cover.transform.parent; parent != transform; parent = parent.parent)
          if (seen.Contains(parent.gameObject))
          { error = "A cover cannot contain another cover."; return false; }
      error = seen.Count == 0 ? "No covers configured." : null;
      return error == null;
    }

#if UNITY_EDITOR
    public void ConfigureAuthoring(string tileId, int cost, List<GameObject> covers)
    {
      _tileId = tileId;
      _hexCoinsCostPerStep = Mathf.Max(0, cost);
      _progressPerStep = 1f;
      var group = new MetaTileObjectGroup();
      group.Objects.AddRange(covers);
      _groups = covers.Count > 0 ? new List<MetaTileObjectGroup> { group } : new List<MetaTileObjectGroup>();
      InitializeFromSave(null);
    }

    public bool MatchesAuthoringCovers(List<GameObject> covers)
    {
      if (GroupsCount != 1 || _groups[0]?.Objects == null || _groups[0].Objects.Count != covers.Count) return false;
      for (int i = 0; i < covers.Count; i++)
        if (_groups[0].Objects[i] != covers[i]) return false;
      return true;
    }

    public void PreviewOpenedCount(int count)
    {
      InitializeFromSave(null);
      for (int i = 0; i < count; i++)
      {
        _currentObjectProgress = 0f;
        if (_currentGroupIndex >= GroupsCount) break;
        _currentObjectIndexInGroup++;
        NormalizeCursor();
      }
      RefreshVisuals();
    }
#endif
  }
}
