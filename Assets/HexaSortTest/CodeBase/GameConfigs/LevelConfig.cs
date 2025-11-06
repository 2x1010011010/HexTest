using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameConfigs
{
  [CreateAssetMenu(fileName = "LevelConfig", menuName = "Static Data/Level Config", order = 52)]
  public class LevelConfig : ScriptableObject
  {
    [field: SerializeField, BoxGroup("VISUAL SETTINGS")] public Material Material { get; private set; }
    [field: SerializeField, BoxGroup("VISUAL SETTINGS")] public List<Color> CellColors { get; private set; }
    
    [field: SerializeField, BoxGroup("GRID SETTINGS")] public GameObject GridPrefab { get; private set; }

    [field: SerializeField, Range(45, 80), BoxGroup("CAMERA SETUP")] public float FieldOfView { get; private set; } = 50f;
  }
}