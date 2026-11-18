using System;
using HexaSortTest.CodeBase.GameLogic.Boosters;
using HexaSortTest.CodeBase.Infrastructure.Services.BoosterInventoryService;
using HexaSortTest.CodeBase.Infrastructure.Services.CurrencyService;
using HexaSortTest.CodeBase.Infrastructure.Services.LivesService;
using HexaSortTest.CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace HexaSortTest.CodeBase.Editor
{
  public sealed class ResourceCheatsWindow : EditorWindow
  {
    [SerializeField] private int _hexCoins = 100;
    [SerializeField] private int _coins = 100;
    [SerializeField] private int _lives = 1;
    [SerializeField] private int _boosters = 1;
    [SerializeField] private BoosterType _boosterType;

    [MenuItem("Tools/HexaSort/Resource Cheats")]
    private static void Open()
    {
      var window = GetWindow<ResourceCheatsWindow>("Resource Cheats");
      window.minSize = new Vector2(360, 230);
    }

    private void OnInspectorUpdate() => Repaint();

    private void OnGUI()
    {
      if (!EditorApplication.isPlaying || !EditorApplication.isPlayingOrWillChangePlaymode ||
          !ProjectContext.HasInstance)
      {
        EditorGUILayout.HelpBox("Start the game in Play Mode to grant resources.", MessageType.Info);
        return;
      }

      var container = ProjectContext.Instance.Container;
      if (container?.TryResolve<IPersistentProgressService>()?.PlayerProgress == null)
      {
        EditorGUILayout.HelpBox("Waiting for player progress to load.", MessageType.Info);
        return;
      }

      var currency = container.Resolve<ICurrencyService>();
      var lives = container.Resolve<LivesService>();
      var inventory = container.Resolve<IBoosterInventoryService>();

      EditorGUILayout.LabelField("Add to current save", EditorStyles.boldLabel);
      DrawGrant("HexCoins", currency.HexCoins, ref _hexCoins, currency.AddHexCoins);
      DrawGrant("Coins", currency.Coins, ref _coins, currency.AddCoins);
      DrawGrant("Lives", lives.Lives, ref _lives, lives.AddLives, lives.MaxLives);

      EditorGUILayout.Space();
      _boosterType = (BoosterType)EditorGUILayout.EnumPopup("Booster", _boosterType);
      DrawGrant(_boosterType.ToString(), inventory.GetCount(_boosterType), ref _boosters,
        amount => inventory.Add(_boosterType, amount));
    }

    private static void DrawGrant(string name, int balance, ref int amount, Action<int> grant,
      int maximum = int.MaxValue)
    {
      using (new EditorGUILayout.HorizontalScope())
      {
        string count = maximum == int.MaxValue ? balance.ToString() : $"{balance}/{maximum}";
        GUILayout.Label($"{name} ({count})", GUILayout.MinWidth(140));
        amount = EditorGUILayout.IntField(amount, GUILayout.MinWidth(55));
        int available = maximum - balance;
        using (new EditorGUI.DisabledScope(amount <= 0 || available <= 0))
        {
          if (GUILayout.Button("Add", GUILayout.Width(60)))
            grant(Math.Min(amount, available));
        }
      }
    }
  }
}
