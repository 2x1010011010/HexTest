namespace HexaSortTest.CodeBase.Infrastructure.Services.ObjectsPoolService
{
  public interface IPoolable
  {
    void OnSpawnedFromPool();
    void OnReturnedToPool();
  }
}