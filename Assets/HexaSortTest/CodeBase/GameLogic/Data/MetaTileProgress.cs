using System;

namespace HexaSortTest.CodeBase.GameLogic.Data
{
  [Serializable]
  public class MetaTileProgress
  {
    public string TileId;
    public int UnlockedObjectsCount;
    public float CurrentObjectProgress;

    public MetaTileProgress(string tileId)
    {
      TileId = tileId;
      UnlockedObjectsCount = 0;
      CurrentObjectProgress = 0f;
    }
  }
}