using System;

namespace HexaSortTest.CodeBase.GameLogic.Data
{
  [Serializable]
  public class LastLevel
  {
    public string Level;

    public LastLevel(string level) => 
      Level = level;
  }
}