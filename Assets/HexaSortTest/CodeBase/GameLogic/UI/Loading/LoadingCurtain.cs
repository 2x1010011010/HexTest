using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.UI.Loading
{
  public class LoadingCurtain : MonoBehaviour
  {
    [SerializeField, BoxGroup("SETUP")] private CanvasGroup _canvasGroup;

    private Coroutine _fadeOut;

    public void Show()
    {
      if (_fadeOut != null) StopCoroutine(_fadeOut);
      _fadeOut = null;
      gameObject.SetActive(true);
      _canvasGroup.alpha = 1;
    }
    
    public void Hide()
    {
      if (!gameObject.activeInHierarchy || _fadeOut != null) return;
      _fadeOut = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
      while (_canvasGroup.alpha > 0)
      {
        _canvasGroup.alpha -= 0.2f;
        yield return new WaitForSeconds(0.03f);
      }
      
      _canvasGroup.alpha = 0;
      _fadeOut = null;
      gameObject.SetActive(false);
    }
  }
}