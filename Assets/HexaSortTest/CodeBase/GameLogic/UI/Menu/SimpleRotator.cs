using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.Menu
{
  [DisallowMultipleComponent]
  public sealed class SimpleRotator : MonoBehaviour
  {
    [SerializeField, BoxGroup("ROTATION"), Required]
    [Tooltip("Root object of the decorative grid, including all tile stacks.")]
    private GameObject _target;

    [SerializeField, BoxGroup("ROTATION")]
    [Tooltip("Rotation axis. For example: (0, 1, 0) around Y; (0, -1, 0) reverses it. Zero stops rotation.")]
    private Vector3 _direction = Vector3.up;

    [SerializeField, BoxGroup("ROTATION"), Min(0f)]
    [Tooltip("Rotation speed in degrees per second.")]
    private float _speed = 10f;

    [SerializeField, BoxGroup("ROTATION")]
    [Tooltip("Self uses the target's local axis; World uses the scene axis.")]
    private Space _rotationSpace = Space.Self;

    [SerializeField, BoxGroup("ROTATION")]
    [Tooltip("Keep the menu background rotating even when Time.timeScale is zero.")]
    private bool _useUnscaledTime = true;

    private void Update()
    {
      if (_target == null || !_target.activeInHierarchy || _speed <= 0f)
        return;

      float deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
      _target.transform.Rotate(_direction.normalized, _speed * deltaTime, _rotationSpace);
    }
  }
}
