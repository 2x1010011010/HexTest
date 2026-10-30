using System;

namespace HexaSortTest.CodeBase.GameLogic.GridLogic
{
  public enum BoardState
  {
    Initializing, Idle, DraggingStack, DroppingStack, Rotating, SnappingRotation,
    SelectingBooster, ApplyingBooster, Settling, Completed, Faulted, Disposed
  }

  public enum BoardOutcome { None, Victory, Defeat }

  // Pure gameplay state. Neither input callbacks nor animation callbacks may finish a level.
  public sealed class BoardSession
  {
    public BoardState State { get; private set; } = BoardState.Initializing;
    public int Score { get; private set; }
    public int Goal { get; private set; }
    public event Action<int> ScoreChanged;
    public event Action<BoardState> StateChanged;

    public void ConfigureGoal(int goal)
    {
      if (State != BoardState.Initializing || goal <= 0)
        throw new InvalidOperationException("Configure a positive goal before starting the board.");
      Goal = goal;
    }

    public bool TryTransition(BoardState expected, BoardState next)
    {
      if (State != expected || !IsAllowed(expected, next)) return false;
      SetState(next);
      return true;
    }

    // Settlement runs independently from pointer gestures in GridObserver.
    public void BeginSettlement()
    {
      if (State == BoardState.Settling) return;
      if (State != BoardState.Initializing && State != BoardState.Idle)
        throw new InvalidOperationException("Cannot settle a finished board.");
      SetState(BoardState.Settling);
    }

    public void AddScore(int count)
    {
      if (count < 0 || (State != BoardState.Settling && State != BoardState.ApplyingBooster))
        throw new InvalidOperationException("Only a running collection operation can award score.");
      Score = checked(Score + count);
      ScoreChanged?.Invoke(Score);
    }

    public BoardOutcome CompleteSettlement(bool boardFull)
    {
      if (State != BoardState.Settling || Goal <= 0)
        throw new InvalidOperationException("Evaluate the result only after settlement.");
      var result = Score >= Goal ? BoardOutcome.Victory : boardFull ? BoardOutcome.Defeat : BoardOutcome.None;
      SetState(result == BoardOutcome.None ? BoardState.Idle : BoardState.Completed);
      return result;
    }

    public void Fail() { if (State != BoardState.Disposed) SetState(BoardState.Faulted); }
    public void Dispose() => SetState(BoardState.Disposed);

    private void SetState(BoardState state)
    {
      State = state;
      StateChanged?.Invoke(state);
    }

    private static bool IsAllowed(BoardState from, BoardState to) => from switch
    {
      BoardState.Initializing => to == BoardState.Settling,
      BoardState.Idle => to == BoardState.DraggingStack || to == BoardState.Rotating ||
        to == BoardState.SelectingBooster || to == BoardState.ApplyingBooster,
      BoardState.DraggingStack => to == BoardState.DroppingStack || to == BoardState.Idle,
      BoardState.DroppingStack => to == BoardState.Settling || to == BoardState.Idle,
      BoardState.Rotating => to == BoardState.SnappingRotation || to == BoardState.Idle,
      BoardState.SnappingRotation => to == BoardState.Idle,
      BoardState.SelectingBooster => to == BoardState.DraggingStack || to == BoardState.ApplyingBooster || to == BoardState.Idle,
      BoardState.ApplyingBooster => to == BoardState.Settling,
      _ => false
    };
  }

  public static class BoardRotationMath
  {
    public const float Step = 60f;
    public static float Snap(float degrees) => (float)Math.Round(degrees / Step, MidpointRounding.AwayFromZero) * Step;
    public static float Normalize(float degrees) => ((degrees + 180f) % 360f + 360f) % 360f - 180f;
  }
}
