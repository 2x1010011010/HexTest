using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameConfigs
{
  [CreateAssetMenu(fileName = "StorePositionBundle", menuName = "Static Data/Store/Store Position Bundle", order = 61)]
  public class StorePositionBundle : ScriptableObject
  {
    [field: SerializeField, BoxGroup("BUNDLE"), Tooltip("Reserved for the future IAP product SKU. Not used yet — IAP isn't wired up in this pass.")]
    public string Sku { get; private set; }

    [field: SerializeField, BoxGroup("BUNDLE")] public string DisplayName { get; private set; }
    [field: SerializeField, BoxGroup("BUNDLE"), TextArea] public string Description { get; private set; }
    [field: SerializeField, BoxGroup("BUNDLE")] public Sprite Icon { get; private set; }

    [field: SerializeField, BoxGroup("BUNDLE"), Tooltip("Shown until real IAP pricing exists.")]
    public string PriceDisplayFallback { get; private set; } = "---";

    [field: SerializeField, BoxGroup("CONTENTS")] public List<StorePositionBundleEntry> Entries { get; private set; }
  }
}