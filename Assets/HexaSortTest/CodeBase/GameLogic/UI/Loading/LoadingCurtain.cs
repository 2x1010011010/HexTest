using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.Loading
{
  public class LoadingCurtain : MonoBehaviour
  {
    [SerializeField, BoxGroup("SETUP")] private CanvasGroup _canvasGroup;

    public void Show()
    {
      gameObject.SetActive(true);
      _canvasGroup.alpha = 1;
    }
    
    public void Hide() => 
      StartCoroutine(FadeOut());

    private IEnumerator FadeOut()
    {
      while (_canvasGroup.alpha > 0)
      {
        _canvasGroup.alpha -= 0.03f;
        yield return new WaitForSeconds(0.03f);
      }
      
      gameObject.SetActive(false);
    }
  }
}