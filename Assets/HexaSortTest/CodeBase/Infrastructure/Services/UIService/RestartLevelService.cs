using System;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.UIService
{
  public class RestartLevelService : IUIListenerService
  {
    public event Action ActionRequired;
    
    public void NotifyActionRequired()
    {
      Debug.Log($"[RestartLevelService] NotifyActionRequired — Subscribers: {ActionRequired?.GetInvocationList().Length ?? 0}");
      ActionRequired?.Invoke();
    }
  }
}