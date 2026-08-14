using System;
using System.Collections.Generic;
using HexaSortTest.CodeBase.GameLogic.Boosters;

namespace HexaSortTest.CodeBase.GameLogic.Data
{
  [Serializable]
  public class PlayerProgress
  {
    public WorldData WorldData;
    public int LevelIndex;

    public int HexCoins;
    public int Coins;

    public List<MetaTileProgress> MetaProgress;
    public List<BoosterInventoryEntry> Boosters;

    public PlayerProgress(string initialLevel)
    {
      WorldData = new WorldData(initialLevel);
      LevelIndex = 0;

      HexCoins = 0;
      Coins = 0;

      MetaProgress = new List<MetaTileProgress>();

      Boosters = new List<BoosterInventoryEntry>
      {
        new BoosterInventoryEntry(BoosterType.Hammer, 2),
        new BoosterInventoryEntry(BoosterType.Hand, 2),
        new BoosterInventoryEntry(BoosterType.Respawn, 2),
      };
    }
  }
}