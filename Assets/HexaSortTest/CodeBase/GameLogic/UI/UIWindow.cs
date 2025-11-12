using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI
{
  public abstract class UIWindow : MonoBehaviour
  {
    public bool IsOpen { get; private set; }
    public virtual void Open()
    {
      IsOpen = true;
      gameObject.SetActive(true);
    }

    public virtual void Close()
    {
      IsOpen = false;
      gameObject.SetActive(false);
    }

    protected virtual void Awake() => Close();
    
  }
}