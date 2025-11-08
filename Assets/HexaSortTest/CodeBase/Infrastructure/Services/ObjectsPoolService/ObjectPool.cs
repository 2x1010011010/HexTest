using System.Collections.Generic;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService
{
  public class ObjectPool<TObject> where TObject : class, IPoolable
  {
    public List<TObject> Pool { get; } = new();

    public void SetToPool(TObject obj)
    {
      
    }
    
    public TObject GetFromPool()
    {
      return null;
    }
    
    public void Clear()
    {
      
    }
  }
}