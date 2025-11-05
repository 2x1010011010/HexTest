using System;
using UnityEngine.Serialization;

namespace HexaSortTest.CodeBase.GameLogic.Data
{
  [Serializable]
  public class WorldData
  {
    public LastLevel LastLevel;
    
    public WorldData(string initialLevel) => 
      LastLevel = new LastLevel(initialLevel);
  }
}