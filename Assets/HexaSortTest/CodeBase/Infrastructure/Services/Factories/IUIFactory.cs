using HexaSortTest.CodeBase.GameLogic.GridLogic;
using HexaSortTest.CodeBase.GameLogic.Meta;
using HexaSortTest.CodeBase.GameLogic.Spawners;
using HexaSortTest.CodeBase.GameLogic.UI.HUD;
using HexaSortTest.CodeBase.GameLogic.UI.MainMenu;
using HexaSortTest.CodeBase.GameLogic.UI.Menu;
using HexaSortTest.CodeBase.GameLogic.UI.Meta;
using HexaSortTest.CodeBase.GameLogic.UI.ResultPopup;
using HexaSortTest.CodeBase.GameLogic.UI.Shop;

namespace HexaSortTest.CodeBase.Infrastructure.Services.Factories
{
  public interface IUIFactory : IFactory
  {
    MainMenuObserver CreateMainMenu();
    void CreateHud(int winCondition, MainMenuObserver mainMenu, StacksSpawner stacksSpawner, GridObserver gridObserver, BoosterPurchasePopup boosterPurchasePopup = null);
    MainMenuScreen CreateMainMenuScreen();
    GameResultPopup CreateGameResultPopup();
    MetaUIObserver CreateMetaUI();
    BoosterPurchasePopup CreateBoosterPurchasePopup();
    ShopSceneObserver CreateShopSceneUI();
    void Clear();
  }
}