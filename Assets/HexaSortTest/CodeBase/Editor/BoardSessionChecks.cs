using System;
using HexaSortTest.CodeBase.GameLogic.GridLogic;

namespace HexaSortTest.CodeBase.Editor
{
  public static class BoardSessionChecks
  {
    public static int Assertions { get; private set; }
    public static void Run()
    {
      Assertions = 0;
      var board = new BoardSession();
      board.ConfigureGoal(10);
      Check(!board.TryTransition(BoardState.Idle, BoardState.Rotating), "Input is blocked during initialization");
      Check(board.TryTransition(BoardState.Initializing, BoardState.Settling), "Initial settle starts");
      Check(board.CompleteSettlement(false) == BoardOutcome.None && board.State == BoardState.Idle, "Stable board becomes interactive");
      Check(board.TryTransition(BoardState.Idle, BoardState.Rotating), "Rotation captures board");
      Check(!board.TryTransition(BoardState.Idle, BoardState.DraggingStack), "No drag while rotating");
      Check(!board.TryTransition(BoardState.Rotating, BoardState.ApplyingBooster), "No booster during rotation");
      Check(board.TryTransition(BoardState.Rotating, BoardState.SnappingRotation), "Release starts snap");
      Check(!board.TryTransition(BoardState.Idle, BoardState.SelectingBooster), "No booster during snap");
      Check(board.TryTransition(BoardState.SnappingRotation, BoardState.Idle), "Snap releases input");
      Check(board.TryTransition(BoardState.Idle, BoardState.SelectingBooster), "Select booster");
      Check(board.TryTransition(BoardState.SelectingBooster, BoardState.Idle), "Cancel selection");
      Check(board.Score == 0, "Selection/cancellation does not change score");
      Check(board.TryTransition(BoardState.Idle, BoardState.DraggingStack), "Begin drag");
      Check(board.TryTransition(BoardState.DraggingStack, BoardState.Idle), "Cancel drag");
      Check(board.TryTransition(BoardState.Idle, BoardState.SelectingBooster), "Select hand");
      Check(board.TryTransition(BoardState.SelectingBooster, BoardState.DraggingStack), "Hand captures drag");
      Check(board.TryTransition(BoardState.DraggingStack, BoardState.DroppingStack), "Valid hand drop animates");
      Check(!board.TryTransition(BoardState.Idle, BoardState.Rotating), "No rotation while dropping");
      Check(board.TryTransition(BoardState.DroppingStack, BoardState.Settling), "Settle after placement");
      board.AddScore(10);
      Check(board.State == BoardState.Settling, "Crossing the goal must not switch scenes mid-animation");
      Check(board.CompleteSettlement(true) == BoardOutcome.Victory, "Victory takes precedence over full board");
      Check(board.State == BoardState.Completed, "Result freezes board");
      Check(!board.TryTransition(BoardState.Completed, BoardState.Idle), "Finished board cannot reopen");
      Throws(() => board.CompleteSettlement(true), "Result cannot be issued twice");
      var defeat = Ready();
      Check(defeat.TryTransition(BoardState.Idle, BoardState.ApplyingBooster), "Start one respawn operation");
      Check(!defeat.TryTransition(BoardState.Idle, BoardState.ApplyingBooster), "Repeated respawn cannot start concurrently");
      Check(defeat.TryTransition(BoardState.ApplyingBooster, BoardState.Settling), "Settle booster result");
      Check(defeat.CompleteSettlement(true) == BoardOutcome.Defeat, "Full stable board loses below goal");
      var canceled = Ready();
      Throws(() => canceled.AddScore(1), "Idle UI cannot award score");
      canceled.TryTransition(BoardState.Idle, BoardState.Rotating);
      Check(canceled.TryTransition(BoardState.Rotating, BoardState.Idle), "Interrupted rotation returns control");
      canceled.Fail();
      Check(!canceled.TryTransition(BoardState.Faulted, BoardState.Idle), "Failed operation cannot accept competing input");
      canceled.Dispose();
      Check(canceled.State == BoardState.Disposed, "Scene teardown ends the session");
      foreach (var item in new[] { (0f, 0f), (29f, 0f), (31f, 60f), (89f, 60f), (91f, 120f),
        (-29f, 0f), (-31f, -60f), (-91f, -120f), (359f, 360f), (721f, 720f), (30f, 60f), (-30f, -60f) })
        Check(BoardRotationMath.Snap(item.Item1) == item.Item2, "Nearest 60-degree position: " + item.Item1);
      for (int angle = -1440; angle <= 1440; angle++)
      {
        float snapped = BoardRotationMath.Snap(angle);
        Check(Math.Abs(snapped - angle) <= 30.001f, "Snap distance");
        Check(Math.Abs(snapped % 60f) < 0.001f, "Exact hexagonal orientation");
        float normalized = BoardRotationMath.Normalize(snapped);
        Check(normalized >= -180f && normalized < 180f && Math.Abs(normalized % 60f) < 0.001f, "No cumulative angle drift");
      }
    }
    private static BoardSession Ready()
    {
      var board = new BoardSession();
      board.ConfigureGoal(10);
      board.TryTransition(BoardState.Initializing, BoardState.Settling);
      board.CompleteSettlement(false);
      return board;
    }
    private static void Check(bool value, string message)
    {
      Assertions++;
      if (!value) throw new Exception("Board check failed: " + message);
    }
    private static void Throws(Action action, string message)
    {
      bool threw = false;
      try { action(); } catch (InvalidOperationException) { threw = true; }
      Check(threw, message);
    }
  }
}
