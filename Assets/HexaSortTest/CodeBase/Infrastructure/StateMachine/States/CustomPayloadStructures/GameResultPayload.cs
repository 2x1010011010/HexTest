namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States.CustomPayloadStructures
{
  public readonly struct GameResultPayload
  {
    public readonly bool IsVictory;

    public GameResultPayload(bool isVictory) =>
      IsVictory = isVictory;
  }
}