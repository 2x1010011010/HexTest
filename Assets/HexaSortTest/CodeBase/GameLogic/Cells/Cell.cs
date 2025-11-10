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
    
    public bool IsSpawner => _isSpawner;
    public bool IsActive { get; private set; }
    public bool IsEmpty { get; private set; }

    public Color Color
    {
      get => _renderer.material.color; 
      set => _renderer.material.color = value;
    }

    public void SetSpawner(bool isSpawner) => 
      _isSpawner = isSpawner;

    public void SetActive(bool isActive)
    {
      IsActive = isActive;
      gameObject.SetActive(isActive);
    }
    
    public void SetEmpty(bool isEmpty) => 
      IsEmpty = isEmpty;

    public void SetParent(Transform parent) => 
      transform.SetParent(parent);
  }
}