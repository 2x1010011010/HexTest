using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HexaSortTest.CodeBase.GameLogic.UI.Meta
{
  // Usable fallback when custom scene UI is not assigned; no prefab dependency.
  internal static class MetaFeedbackUI
  {
    public static GameObject Create(ref MetaHexCoinsView counter, ref MetaInsufficientFundsPopup popup)
    {
      var root = new GameObject("Meta Feedback UI", typeof(RectTransform), typeof(Canvas),
        typeof(CanvasScaler), typeof(GraphicRaycaster));
      var canvas = root.GetComponent<Canvas>();
      canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      canvas.sortingOrder = 100;
      var scaler = root.GetComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(1080, 1920);
      scaler.matchWidthOrHeight = 0.5f;
      if (counter == null)
      {
        var panel = Rect("HexCoins", root.transform, new Vector2(0.5f, 1f), new Vector2(400, 100), new Vector2(0, -150));
        var background = panel.gameObject.AddComponent<Image>();
        background.color = new Color(0.12f, 0.16f, 0.26f, 0.95f);
        background.raycastTarget = false;
        Text("Label", panel, "HexCoins", new Vector2(0, 22), new Vector2(380, 40), 26);
        var value = Text("Value", panel, "0", new Vector2(0, -22), new Vector2(380, 45), 34);
        counter = panel.gameObject.AddComponent<MetaHexCoinsView>();
        counter.Configure(value);
      }
      if (popup == null)
      {
        var overlay = Rect("Insufficient HexCoins", root.transform, Vector2.one * 0.5f, Vector2.zero, Vector2.zero);
        overlay.gameObject.SetActive(false);
        overlay.anchorMin = Vector2.zero;
        overlay.anchorMax = Vector2.one;
        overlay.offsetMin = overlay.offsetMax = Vector2.zero;
        var shade = overlay.gameObject.AddComponent<Image>();
        shade.color = new Color(0, 0, 0, 0.65f);
        var panel = Rect("Panel", overlay, Vector2.one * 0.5f, new Vector2(820, 560), Vector2.zero);
        panel.gameObject.AddComponent<Image>().color = new Color(0.15f, 0.19f, 0.29f);
        var message = Text("Message", panel, "", new Vector2(0, 65), new Vector2(760, 350), 40);
        var buttonRect = Rect("Close", panel, Vector2.one * 0.5f, new Vector2(420, 90), new Vector2(0, -180));
        var image = buttonRect.gameObject.AddComponent<Image>();
        image.color = new Color(0.3f, 0.5f, 0.7f);
        var button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        Text("Label", buttonRect, "OK", Vector2.zero, new Vector2(400, 80), 36);
        popup = overlay.gameObject.AddComponent<MetaInsufficientFundsPopup>();
        var closeButton = buttonRect.gameObject.AddComponent<MetaPopupCloseButton>();
        closeButton.Configure(button);
        popup.Configure(message, closeButton);
      }
      return root;
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 size, Vector2 position)
    {
      var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
      rect.SetParent(parent, false);
      rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
      rect.sizeDelta = size;
      rect.anchoredPosition = position;
      return rect;
    }
    private static TextMeshProUGUI Text(string name, Transform parent, string value, Vector2 position, Vector2 size, float fontSize)
    {
      var text = Rect(name, parent, Vector2.one * 0.5f, size, position).gameObject.AddComponent<TextMeshProUGUI>();
      text.font = TMP_Settings.defaultFontAsset;
      text.text = value;
      text.fontSize = fontSize;
      text.color = Color.white;
      text.alignment = TextAlignmentOptions.Center;
      text.raycastTarget = false;
      return text;
    }
  }
}
