using System;
using Cysharp.Threading.Tasks;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using HexaSortTest.CodeBase.GameLogic.Spawners;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using HexaSortTest.CodeBase.Infrastructure.Services.BoosterInventoryService;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Boosters
{
  public class BoosterTools : MonoBehaviour
  {
    private GridObserver _board;
    private StacksSpawner _spawner;
    private IBoosterInventoryService _inventory;
    private IBooster _selected;
    public void Initialize(GridObserver board, StacksSpawner spawner, IBoosterInventoryService inventory)
    {
      _board = board;
      _spawner = spawner;
      _inventory = inventory;
      board.AttachBoosters(this, spawner);
    }

    public void ActivateBooster(IBooster booster)
    {
      if (booster == null || !_board.CanChooseBooster || _inventory.GetCount(booster.Type) <= 0) return;
      if (_selected != null && _selected.Type == booster.Type)
      {
        _board.CancelInteraction();
        return;
      }
      _selected = booster;
      if (booster.Type == BoosterType.Respawn)
      {
        if (!_spawner.HasTrayStacks) { _board.CancelInteraction(); ClearSelection(); return; }
        _board.ExecuteBoosterAsync(async token =>
        {
          await _spawner.ReplaceTrayAsync(token);
          Spend(BoosterType.Respawn);
          ClearSelection();
        }).Forget();
      }
      else _board.SelectBooster();
    }

    // Returns true only when the pointer is captured for a hand drag.
    public bool TryChooseTarget(Stack stack, Vector2 screen, Camera camera)
    {
      if (_selected == null) return false;
      if (!_board.IsPlacedStack(stack)) { _board.CancelInteraction(); return false; }
      if (_inventory.GetCount(_selected.Type) <= 0) { _board.CancelInteraction(); return false; }
      if (_selected.Type == BoosterType.Hand)
        return _board.TryBeginDrag(stack, screen, camera, true);
      if (_selected.Type == BoosterType.Hammer)
      {
        var cell = stack.Cell;
        _board.ExecuteBoosterAsync(async token =>
        {
          int removed = await stack.RemoveAllAsync(token, _board.PresentRemovalProgress);
          if (removed <= 0) throw new InvalidOperationException("The selected stack is empty.");
          Spend(BoosterType.Hammer);
          ClearSelection();
          await _board.OnCollectedAsync(cell, removed, token);
        }).Forget();
      }
      return false;
    }

    public bool TrySpendHand() => _inventory.TrySpend(BoosterType.Hand);
    public void ClearSelection() => _selected = null;
    private void Spend(BoosterType type)
    {
      if (!_inventory.TrySpend(type)) throw new InvalidOperationException("Booster inventory changed during an exclusive board operation.");
    }
  }
}
