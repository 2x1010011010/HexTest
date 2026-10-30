namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States.CustomPayloadStructures
{
  public readonly struct GameResultPayload
  {
    public readonly bool IsVictory;
    public readonly int Score;

    public GameResultPayload(bool isVictory, int score = 0)
    {
      IsVictory = isVictory;
      Score = score;
    }
  }
}
