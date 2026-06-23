using System;

namespace HexaSortTest.CodeBase.GameLogic.Data
{
  [Serializable]
  public class PlayerProgress
  {
    public WorldData WorldData;
    public int LevelIndex;

    public PlayerProgress(string initialLevel)
    {
      WorldData = new WorldData(initialLevel);
      LevelIndex = 0;
    }
  }
}