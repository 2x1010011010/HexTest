using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.InputService
{
  public interface IInputService : IService
  {
    Vector2 Axis { get; }
    Vector3? LookDirection { get; }
    bool IsAttackButtonDown();
  }
}