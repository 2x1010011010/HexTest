using System;
using HexaSortTest.CodeBase.GameLogic.Boosters;

namespace HexaSortTest.CodeBase.GameLogic.Data
{
  [Serializable]
  public class BoosterInventoryEntry
  {
    public BoosterType Type;
    public int Count;

    public BoosterInventoryEntry(BoosterType type, int count)
    {
      Type = type;
      Count = count;
    }
  }
}