using HexaSortTest.CodeBase.GameConfigs;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HexaSortTest.CodeBase.GameLogic.UI.Shop
{
  public class StorePositionBundleListItem : MonoBehaviour
  {
    [SerializeField, BoxGroup("SETUP")] private Image _icon;
    [SerializeField, BoxGroup("SETUP")] private TMP_Text _nameText;
    [SerializeField, BoxGroup("SETUP")] private TMP_Text _descriptionText;
    [SerializeField, BoxGroup("SETUP")] private TMP_Text _priceText;

    public StorePositionBundle Bundle { get; private set; }

    public void Setup(StorePositionBundle bundle)
    {
      Bundle = bundle;

      if (bundle == null)
        return;

      if (_icon != null && bundle.Icon != null) _icon.sprite = bundle.Icon;
      if (_nameText != null) _nameText.text = bundle.DisplayName;
      if (_descriptionText != null) _descriptionText.text = bundle.Description;
      if (_priceText != null) _priceText.text = bundle.PriceDisplayFallback;
    }
  }
}