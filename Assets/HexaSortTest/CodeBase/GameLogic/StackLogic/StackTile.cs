using HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.StackLogic
{
  public class StackTile : MonoBehaviour, IPoolable
  {
    [SerializeField, BoxGroup("SETUP")] private Renderer _renderer;
    private bool _isActive;
    
    public Color Color
    {
      get => _renderer.material.color; 
      set => _renderer.material.color = value;
    }
    
    public bool IsActive => _isActive;
    
    public void SetActive(bool isActive)
    {
      _isActive = isActive;
      gameObject.SetActive(isActive);
    }

    public void SetParent(Transform parent) => 
      transform.SetParent(parent);
  }
}