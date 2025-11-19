using HexaSortTest.CodeBase.GameLogic.StackLogic;
using HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Cells
{
  public class Cell : MonoBehaviour
  {
    [SerializeField, BoxGroup("SETUP")] private Renderer _renderer;
    [Space]
    [SerializeField, Tooltip("Set this on or off only for level grid"), BoxGroup("SET CELL SPAWNER")] private bool _isSpawner;
    [SerializeField, BoxGroup("SET EMPTY")] private bool _isEmpty;
    
    private Color _defaultColor;
    
    public bool IsSpawner => _isSpawner;
    public bool IsEmpty => _isEmpty;
    
    public Color Color
    {
      get => _renderer.material.color; 
      set => _renderer.material.color = value;
    }
    
    private void Awake() => _defaultColor = Color;

    public void SetSpawner(bool isSpawner) => 
      _isSpawner = isSpawner;
    
    public void ShineOn() => Color = Color.white;
    public void ShineOff() => Color = _defaultColor;
    
    public void SetEmpty(bool isEmpty) => 
      _isEmpty = isEmpty;
  }
}