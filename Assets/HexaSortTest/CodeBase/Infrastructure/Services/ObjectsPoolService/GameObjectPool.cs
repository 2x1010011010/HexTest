using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService
{
  public class GameObjectPool : MonoBehaviour
  {
    private readonly List<GameObject> _pool = new();
    private Transform _container;

    public void SetPool(GameObject prefab, Transform container,int count)
    {
      _container = container;
      for (int i = 0; i < count; i++)
      {
        var go = Instantiate(prefab, container.position, Quaternion.identity, container);
        go.SetActive(false);
        _pool.Add(go);
      }
    }

    public bool TryGetObject(out GameObject gameObject)
    {
      gameObject = _pool.FirstOrDefault(go => !go.activeSelf);
      return gameObject != null;
    }

    public void ReturnObject(GameObject gameObject)
    {
      gameObject.SetActive(false);
      gameObject.transform.SetParent(_container);
      gameObject.transform.localPosition = Vector3.zero;
    }

    public void Clear() => _pool.Clear();
  }
}