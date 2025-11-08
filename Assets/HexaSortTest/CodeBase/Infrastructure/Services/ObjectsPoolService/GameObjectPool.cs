using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService
{
  public class GameObjectPool : MonoBehaviour
  {
    private readonly List<GameObject> _pool = new();
    private Transform _container;

    protected void SetPool(GameObject prefab, Transform container, int count)
    {
      _container = container;
      for (int i = 0; i < count; i++)
      {
        var go = Instantiate(prefab, container);
        go.SetActive(false);
        _pool.Add(go);
      }
    }

    protected GameObject GetObject()
    {
      var gameObject = _pool.FirstOrDefault(go => !go.activeSelf);
      _pool.Remove(gameObject);
      return gameObject;
    }

    public void ReturnObject(GameObject gameObject)
    {
      gameObject.SetActive(false);
      gameObject.transform.SetParent(_container);
      _pool.Add(gameObject);
    }

    public void Clear() => _pool.Clear();
  }
}