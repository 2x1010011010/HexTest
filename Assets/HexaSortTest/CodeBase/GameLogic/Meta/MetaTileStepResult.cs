namespace HexaSortTest.CodeBase.GameLogic.Meta
{
  public readonly struct MetaTileStepResult
  {
    public readonly bool Success;
    public readonly bool ObjectRevealed;
    public readonly bool TileFullyOpened;

    public MetaTileStepResult(bool success, bool objectRevealed, bool tileFullyOpened)
    {
      Success = success;
      ObjectRevealed = objectRevealed;
      TileFullyOpened = tileFullyOpened;
    }

    public static readonly MetaTileStepResult None = new(false, false, false);
  }
}