using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.InputService
{
  public class DesktopInputService : IInputService
  {
    public Vector2 Axis => 
      new Vector2 (Input.GetAxis(Constants.HorizontalAxis), Input.GetAxis(Constants.VerticalAxis));

    public Vector3? LookDirection
    {
      get
      {
        Camera camera = Camera.main;
        if (camera == null)
          return null;

        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (plane.Raycast(ray, out float distance))
          return ray.GetPoint(distance);

        return null;
      }
    }

    public bool IsAttackButtonDown() => 
      Input.GetButtonDown(Constants.FireButton);
  }
}