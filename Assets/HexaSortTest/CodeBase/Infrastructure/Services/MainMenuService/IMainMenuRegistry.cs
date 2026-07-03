using HexaSortTest.CodeBase.GameLogic.UI.Menu;

namespace HexaSortTest.CodeBase.Infrastructure.Services.MainMenuService
{
  public interface IMainMenuRegistry : IService
  {
    MainMenuScreen Screen { get; }
    void Register(MainMenuScreen screen);
    void Clear();
  }
}