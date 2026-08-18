using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameConfigs
{
  [Serializable]
  public class StorePositionBundleEntry
  {
    [SerializeField, BoxGroup("ENTRY")] private StorePosition _storePosition;
    [SerializeField, BoxGroup("ENTRY"), Min(1)] private int _quantity = 1;

    public StorePosition StorePosition => _storePosition;
    public int Quantity => _quantity;
  }
}