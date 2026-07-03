using HexaSortTest.CodeBase.GameLogic.UI.Menu;

namespace HexaSortTest.CodeBase.Infrastructure.Services.MainMenuService
{
  public class MainMenuRegistry : IMainMenuRegistry
  {
    
    public MainMenuScreen Screen { get; private set; }

    public void Register(MainMenuScreen screen) =>
      Screen = screen;

    public void Clear() =>
      Screen = null;
  }
}