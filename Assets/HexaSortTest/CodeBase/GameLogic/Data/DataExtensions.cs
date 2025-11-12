using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Data
{
  public static class DataExtensions
  {
    public static Vector3 AddVertical(this Vector3 vector, float y)
    {
      vector.y += y;
      return vector;
    }
    
    public static bool IsDestroyed(this Object obj)
    {
      return obj == null || obj.Equals(null);
    }

    public static string ToJson(this object obj) => 
      JsonUtility.ToJson(obj);
    
    public static T ToDeserialized<T>(this string json) => 
      JsonUtility.FromJson<T>(json);
    
    
  }
}