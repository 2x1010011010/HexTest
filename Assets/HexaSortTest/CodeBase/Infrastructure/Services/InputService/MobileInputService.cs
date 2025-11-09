using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.InputService
{
  public class MobileInputService : IInputService
  {
    public bool Click()
    {
      Touch click = Input.GetTouch(0);
      return click.phase == TouchPhase.Began;
    }

    public bool Hold()
    {
      Touch click = Input.GetTouch(0);
      return click.phase == TouchPhase.Stationary;
    }

    public bool Release()
    {
      Touch click = Input.GetTouch(0);
      return click.phase == TouchPhase.Ended;
    }
  }
}