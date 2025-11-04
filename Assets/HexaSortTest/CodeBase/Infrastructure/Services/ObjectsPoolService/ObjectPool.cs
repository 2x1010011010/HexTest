using System.Collections.Generic;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService
{
  public class ObjectPool<T> where T : MonoBehaviour, IPoolable
  {
    private readonly Queue<T> _pool = new();
    private readonly T _prefab;
    private readonly Transform _parent;

    public ObjectPool(T prefab, int initialCount, Transform parent = null)
    {
      _prefab = prefab;
      _parent = parent;

      for (int i = 0; i < initialCount; i++)
      {
        T obj = GameObject.Instantiate(_prefab, _parent);
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
      }
    }

    public T Get()
    {
      if (_pool.Count == 0)
      {
        T newObj = GameObject.Instantiate(_prefab, _parent);
        newObj.gameObject.SetActive(false);
        _pool.Enqueue(newObj);
      }

      T obj = _pool.Dequeue();
      obj.gameObject.SetActive(true);
      obj.OnSpawnedFromPool();
      return obj;
    }

    public void Return(T obj)
    {
      obj.OnReturnedToPool();
      obj.gameObject.SetActive(false);
      _pool.Enqueue(obj);
    }

    public void Clear() => _pool.Clear();
  }
}