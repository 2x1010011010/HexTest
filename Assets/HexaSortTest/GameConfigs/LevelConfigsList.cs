using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.GameConfigs
{
  [CreateAssetMenu(fileName = "LevelConfigsList", menuName = "Static Data/Configs List", order = 51)]
  public class LevelConfigsList : ScriptableObject
  {
    [field: SerializeField, BoxGroup("LEVELS")] public List<LevelConfig> Levels {get; private set;}
  }
}