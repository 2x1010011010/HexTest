using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameConfigs
{
  [CreateAssetMenu(fileName = "BoosterShopConfig", menuName = "Static Data/Booster Shop Config", order = 54)]
  public class BoosterShopConfig : ScriptableObject
  {
    [field: SerializeField, BoxGroup("PRICES")] public List<BoosterPriceEntry> Prices { get; private set; }
  }
}