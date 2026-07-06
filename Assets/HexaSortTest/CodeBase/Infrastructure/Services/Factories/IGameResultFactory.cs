using HexaSortTest.CodeBase.GameLogic.UI.ResultPopup;

namespace HexaSortTest.CodeBase.Infrastructure.Services.Factories
{
  public interface IGameResultFactory : IFactory
  {
    GameResultPopup CreateGameResultPopup();
    void Clear();
  }
}