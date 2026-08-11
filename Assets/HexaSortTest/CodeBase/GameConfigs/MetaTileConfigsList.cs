using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameConfigs
{
  [CreateAssetMenu(fileName = "MetaTileConfigsList", menuName = "Static Data/Meta Tile Configs List", order = 53)]
  public class MetaTileConfigsList : ScriptableObject
  {
    [field: SerializeField, BoxGroup("META TILES")] public List<MetaTileConfig> Tiles { get; private set; }
  }
}