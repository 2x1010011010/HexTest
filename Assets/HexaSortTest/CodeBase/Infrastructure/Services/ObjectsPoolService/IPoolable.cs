using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService
{
  public interface IPoolable
  {
    public bool IsActive { get; }
    void SetActive(bool isActive);
    void SetParent(Transform parent);
  }
}