using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace HexaSortTest.CodeBase.Infrastructure.Logging
{
  public static class GameLogger
  {
    private const string MessageColor = "#4CD964";
    private const string WarningColor = "#FFD60A";
    private const string ErrorColor   = "#FF3B30";

    [Conditional("UNITY_EDITOR")]
    public static void Message(string message, Object context = null) =>
      Log(Category.Message, message, context);

    [Conditional("UNITY_EDITOR")]
    public static void Warning(string message, Object context = null) =>
      Log(Category.Warning, message, context);

    [Conditional("UNITY_EDITOR")]
    public static void Error(string message, Object context = null) =>
      Log(Category.Error, message, context);

    private enum Category
    {
      Message,
      Warning,
      Error
    }

    private static void Log(Category category, string message, Object context)
    {
      var (color, tag) = category switch
      {
        Category.Message => (MessageColor, "MSG"),
        Category.Warning => (WarningColor, "WARN"),
        Category.Error   => (ErrorColor, "ERR"),
        _                 => ("#FFFFFF", "")
      };

      string formatted = $"<color={color}><b>[{tag}]</b> {message}</color>";

      switch (category)
      {
        case Category.Message:
          Debug.Log(formatted, context);
          break;
        case Category.Warning:
          Debug.LogWarning(formatted, context);
          break;
        case Category.Error:
          Debug.LogError(formatted, context);
          break;
      }
    }
  }
}
