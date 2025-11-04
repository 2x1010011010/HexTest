using System;

namespace HexaSortTest.CodeBase.GameLogic.Data
{
  [Serializable]
  public class WorldData
  {
    public PositionOnLevel PositionOnLevel;
    
    public WorldData(string initialLevel)
    {
      PositionOnLevel = new PositionOnLevel(initialLevel);
    }
  }
}