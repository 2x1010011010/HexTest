using HexaSortTest.CodeBase.GameLogic.UI.HUD;

namespace HexaSortTest.CodeBase.Infrastructure.Services.GameResultService
{
  public interface IGameResultPopupRegistry : IService
  {
    GameResultPopup Popup { get; }
    void Register(GameResultPopup popup);
    void Clear();
  }
}