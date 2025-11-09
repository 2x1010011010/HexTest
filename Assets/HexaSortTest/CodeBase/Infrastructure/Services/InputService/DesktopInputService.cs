using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.InputService
{
  public class DesktopInputService : IInputService
  {
    public bool Click() => 
      Input.GetMouseButtonDown(0);

    public bool Hold() => 
      Input.GetMouseButton(0);

    public bool Release() => 
      Input.GetMouseButtonUp(0);
  }
}