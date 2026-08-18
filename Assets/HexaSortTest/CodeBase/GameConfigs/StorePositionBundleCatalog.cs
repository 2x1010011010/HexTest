using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameConfigs
{
  [CreateAssetMenu(fileName = "StorePositionBundleCatalog", menuName = "Static Data/Store/Store Position Bundle Catalog", order = 62)]
  public class StorePositionBundleCatalog : ScriptableObject
  {
    [field: SerializeField, BoxGroup("BUNDLES")] public List<StorePositionBundle> Bundles { get; private set; }
  }
}