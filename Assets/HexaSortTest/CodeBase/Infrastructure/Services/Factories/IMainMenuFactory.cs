using HexaSortTest.CodeBase.GameLogic.UI.Menu;

namespace HexaSortTest.CodeBase.Infrastructure.Services.Factories
{
  public interface IMainMenuFactory : IFactory
  {
    MainMenuScreen CreateMainMenuScreen();
    void Clear();
  }
}