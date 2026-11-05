using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HexaSortTest.CodeBase.GameLogic.UI.Meta
{
  public class MetaInsufficientFundsPopup : MonoBehaviour
  {
    [SerializeField] private TMP_Text _message;
    [SerializeField] private Button _closeButton;
    public bool IsOpen => gameObject.activeInHierarchy;
    private void OnEnable() { if (_closeButton != null) _closeButton.onClick.AddListener(Close); }
    private void OnDisable() { if (_closeButton != null) _closeButton.onClick.RemoveListener(Close); }
    public void Configure(TMP_Text message, Button closeButton)
    {
      _message = message;
      _closeButton = closeButton;
      Close();
    }
    public void Show(int cost, int balance)
    {
      if (_message != null)
        _message.text = $"Not enough HexCoins\n\nRequired: {cost}\nBalance: {balance}\nMissing: {Mathf.Max(0, cost - balance)}";
      gameObject.SetActive(true);
      transform.SetAsLastSibling();
    }
    public void Close() => gameObject.SetActive(false);
  }
}
