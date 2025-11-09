using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.InputService
{
  public interface IInputService : IService
  {
    bool Click();
    bool Hold();
    bool Release();
  }
}