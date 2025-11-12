using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI
{
  public class UIPanel : MonoBehaviour
  {
    public bool IsOpen { get; protected set; }
    
    private void Awake() => Close();
    
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
  }
}