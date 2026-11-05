using System;
using System.Collections.Generic;

namespace HexaSortTest.CodeBase.GameLogic.Data
{
  [Serializable]
  public class MetaTileProgress
  {
    public string TileId;
    public bool UsesCellIds;
    public List<string> OpenedCellIds = new();
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