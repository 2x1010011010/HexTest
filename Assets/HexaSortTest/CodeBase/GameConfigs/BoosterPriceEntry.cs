using System;
using HexaSortTest.CodeBase.GameLogic.Boosters;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameConfigs
{
  [Serializable]
  public class BoosterPriceEntry
  {
    [SerializeField, BoxGroup("BOOSTER")] private BoosterType _boosterType;
    [SerializeField, BoxGroup("BOOSTER")] private string _displayName;
    [SerializeField, BoxGroup("BOOSTER")] private Sprite _icon;

    [SerializeField, BoxGroup("PRICE")] private int _priceInCoins;

    public BoosterType BoosterType => _boosterType;
    public string DisplayName => _displayName;
    public Sprite Icon => _icon;
    public int PriceInCoins => _priceInCoins;
  }
}