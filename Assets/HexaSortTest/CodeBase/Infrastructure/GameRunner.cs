using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure
{
  public class GameRunner : MonoBehaviour
  {
    [SerializeField] private Bootstrapper _bootstrapperPrefab;

    private void Awake()
    {
      var bootstrapper = FindObjectOfType<Bootstrapper>();
      if (bootstrapper != null) return;
      if (_bootstrapperPrefab == null) return;
      Instantiate(_bootstrapperPrefab);
    }
  }
}