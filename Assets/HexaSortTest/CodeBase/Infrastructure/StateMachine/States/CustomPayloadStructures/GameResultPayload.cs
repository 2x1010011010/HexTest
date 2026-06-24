using HexaSortTest.CodeBase.GameLogic.UI.HUD;
using HexaSortTest.CodeBase.GameLogic.UI.ResultPopup;

namespace HexaSortTest.CodeBase.Infrastructure.StateMachine.States.CustomPayloadStructures
{
  public struct GameResultPayload
  {
    public readonly bool IsVictory;
    public readonly GameResultPopup Popup;

    public GameResultPayload(bool isVictory, GameResultPopup popup)
    {
      IsVictory = isVictory;
      Popup = popup;
    }
  }
}