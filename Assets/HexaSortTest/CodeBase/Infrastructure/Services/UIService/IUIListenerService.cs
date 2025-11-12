using System;

namespace HexaSortTest.CodeBase.Infrastructure.Services.UIService
{
  public interface IUIListenerService : IService
  {
    event Action ActionRequired;
    void NotifyActionRequired();
  }
}