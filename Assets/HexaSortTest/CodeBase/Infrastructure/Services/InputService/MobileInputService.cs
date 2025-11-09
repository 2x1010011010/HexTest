using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.InputService
{
  public class MobileInputService : IInputService
  {
    public Vector2 Axis { get; }
    public Vector3? LookDirection { get; }
    public bool IsAttackButtonDown()
    {
      throw new System.NotImplementedException();
    }
  }
}