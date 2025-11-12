using System;

namespace HexaSortTest.CodeBase.Infrastructure.Services.UIService
{
  public class RestartLevelService : IUIListenerService
  {
    public event Action ActionRequired;
    
    public void NotifyActionRequired()
    {
      ActionRequired?.Invoke();
    }
  }
}