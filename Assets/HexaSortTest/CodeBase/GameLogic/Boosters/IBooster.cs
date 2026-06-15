using System.Threading.Tasks;

namespace HexaSortTest.CodeBase.GameLogic.Boosters
{
  using HexaSortTest.CodeBase.GameLogic.StackLogic;

  public interface IBooster
  {
    Task BoosterAction(Stack target);
  }
}