using HexaSortTest.CodeBase.GameLogic.StackLogic;
using HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Cells
{
  public class Cell : MonoBehaviour, IPoolable
  {
    [SerializeField, BoxGroup("SETUP")] private Renderer _renderer;
    [Space]
    [SerializeField, Tooltip("Set this on or off only for level grid"), BoxGroup("SET CELL SPAWNER")] private bool _isSpawner;
    [SerializeField, BoxGroup("SET ACTIVE")] private bool _isActive;
    [SerializeField, BoxGroup("SET EMPTY")] private bool _isEmpty;
    
    private Color _defaultColor;
    
    public bool IsSpawner => _isSpawner;
    public bool IsActive => _isActive;
    public bool IsEmpty => _isEmpty;
    public Stack Stack => GetComponentInChildren<Stack>();
    
    public Color Color
    {
      get => _renderer.material.color; 
      set => _renderer.material.color = value;
    }
    
    private void Awake() => _defaultColor = Color;

    public void SetSpawner(bool isSpawner) => 
      _isSpawner = isSpawner;

    public void SetActive(bool isActive)
    {
      _isActive = isActive;
      gameObject.SetActive(isActive);
    }
    
    public void ShineOn() => Color = Color.white;
    public void ShineOff() => Color = _defaultColor;
    
    public void SetEmpty(bool isEmpty) => 
      _isEmpty = isEmpty;

    public void SetParent(Transform parent) => 
      transform.SetParent(parent);
  }
}