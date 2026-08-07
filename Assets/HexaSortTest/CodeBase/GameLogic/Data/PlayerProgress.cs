using System;
using System.Collections.Generic;

namespace HexaSortTest.CodeBase.GameLogic.Data
{
  [Serializable]
  public class PlayerProgress
  {
    public WorldData WorldData;
    public int LevelIndex;
    public int Currency;

    public List<MetaTileProgress> MetaProgress;

    public PlayerProgress(string initialLevel)
    {
      WorldData = new WorldData(initialLevel);
      LevelIndex = 0;
      Currency = 0;
      MetaProgress = new List<MetaTileProgress>();
    }
  }
}