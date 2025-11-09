using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure.Services.AssetManagement
{
  public interface IAssetProvider : IService
  {
    GameObject Instantiate(string path);
    GameObject Instantiate(string path, Transform at);
  }
}