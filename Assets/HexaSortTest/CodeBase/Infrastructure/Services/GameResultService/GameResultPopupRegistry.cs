using HexaSortTest.CodeBase.GameLogic.UI.HUD;

namespace HexaSortTest.CodeBase.Infrastructure.Services.GameResultService
{
  public class GameResultPopupRegistry : IGameResultPopupRegistry
  {
    public GameResultPopup Popup { get; private set; }

    public void Register(GameResultPopup popup) => 
      Popup = popup;

    public void Clear() => 
      Popup = null;
  }
}