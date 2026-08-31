using System;

namespace HexaSortTest.CodeBase.GameLogic.Data
{
  [Serializable]
  public class MetaTileProgress
  {
    public string TileId;
    public int UnlockedGroupsCount;
    public int UnlockedObjectsInGroupCount;
    public float CurrentObjectProgress;

    public MetaTileProgress(string tileId)
    {
      TileId = tileId;
      UnlockedGroupsCount = 0;
      UnlockedObjectsInGroupCount = 0;
      CurrentObjectProgress = 0f;
    }
  }
}