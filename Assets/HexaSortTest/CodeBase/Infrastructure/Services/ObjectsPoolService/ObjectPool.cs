using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService
{
  public class ObjectPool<TObject> where TObject : class, IPoolable  
  {
    private Transform _container;
    private readonly List<TObject> _pool = new();

    public ObjectPool(Transform container) => 
      _container = container;

    public void AddToPool(TObject poolableObject)
    {
      _pool.Add(poolableObject);
      poolableObject.SetParent(_container);
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