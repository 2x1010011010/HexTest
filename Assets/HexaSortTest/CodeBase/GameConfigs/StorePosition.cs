using HexaSortTest.CodeBase.GameLogic.Boosters;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameConfigs
{
  [CreateAssetMenu(fileName = "StorePosition", menuName = "Static Data/Store/Store Position", order = 60)]
  public class StorePosition : ScriptableObject
  {
    [field: SerializeField, BoxGroup("STORE POSITION")] public string Id { get; private set; }
    [field: SerializeField, BoxGroup("STORE POSITION")] public string DisplayName { get; private set; }
    [field: SerializeField, BoxGroup("STORE POSITION")] public Sprite Icon { get; private set; }
    [field: SerializeField, BoxGroup("STORE POSITION")] public StorePositionType Type { get; private set; }

    [field: SerializeField, BoxGroup("AMOUNT"), Min(1),
            Tooltip("Coins/HexCoins amount, booster count, or lives count — depends on Type.")]
    public int Amount { get; private set; } = 1;

    [field: SerializeField, BoxGroup("BOOSTER (used only if Type = Booster)")]
    public BoosterType BoosterType { get; private set; }
  }
}