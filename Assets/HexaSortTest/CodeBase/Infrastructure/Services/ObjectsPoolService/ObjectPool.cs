using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService
{
  public class ObjectPool<TObject> where TObject : class, IPoolable  
  {
    private readonly List<TObject> _pool = new();

    public void AddToPool(TObject poolableObject)
    {
      _pool.Add(poolableObject);
      poolableObject.SetActive(false);
    }

    public bool TryGetObject(out TObject instance)
    {
      instance = _pool.FirstOrDefault(p => p.IsActive == false);
      _pool.Remove(instance);
      return instance != null;
    }

    public void ReturnObject(TObject poolableObject) => 
      AddToPool(poolableObject);

    public void Clear() => 
      _pool.Clear();
  }
}