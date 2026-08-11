using System;
using HexaSortTest.CodeBase.GameLogic.Meta;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameConfigs
{
  [Serializable]
  public class MetaTileConfig
  {
    [SerializeField, BoxGroup("META TILE")] private Sprite _icon;
    [SerializeField, BoxGroup("META TILE")] private MetaTile _tilePrefab;

    public Sprite Icon => _icon;
    public MetaTile TilePrefab => _tilePrefab;
  }
}