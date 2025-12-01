using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HexaSortTest.CodeBase.GameLogic.Cells;
using HexaSortTest.CodeBase.GameLogic.Data;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;
using Sirenix.OdinInspector;
using UnityEngine;
using HexaSortTest.CodeBase.GameLogic.StackLogic;
using HexaSortTest.CodeBase.GameLogic.UI;
using HexaSortTest.CodeBase.GameLogic.UI.MainMenu;
using Cysharp.Threading.Tasks;

namespace HexaSortTest.CodeBase.GameLogic.GridLogic
{
  /// <summary>
  /// Наблюдатель за состоянием гекса-сетки.
  /// Отслеживает появление стеков, запускает слияния, перемещения тайлов,
  /// проверяет условия поражения.
  /// </summary>
  public class GridObserver : MonoBehaviour
  {
    [SerializeField, BoxGroup("SETUP")] private HexGrid _grid; // Сетка гекса-клеток

    [SerializeField, BoxGroup("TILES MOVEMENT ANIMATION SETTINGS")]
    float _pauseBetween = 0.2f; // Пауза между перелётами тайлов

    [SerializeField, BoxGroup("TILES MOVEMENT ANIMATION SETTINGS")]
    float _moveDuration = 0.4f; // Длительность одного перелёта

    private readonly Dictionary<Cell, List<Cell>> _neighbors = new();
    // Список соседей для каждой клетки (кэшируется для скорости)

    private readonly HashSet<Stack> _stacksOnGrid = new();
    // Список всех стеков, уже стоящих на клетках

    private Cell _lastAddedCell; // Клетка, в которую игрок положил стек
    private UIWindow _mainMenu; // Окно проигрыша

    /// <summary>
    /// Сохраняем ссылку на главное меню для показа при проигрыше.
    /// </summary>
    public void SetMainMenu(MainMenuObserver mainMenu) => _mainMenu = mainMenu;

    /// <summary>
    /// Инициализация внешней HexGrid.
    /// </summary>
    public void Init(HexGrid grid) => _grid = grid;

    /// <summary>
    /// Предвычисляет соседей, сканирует существующие стеки и запускает первичную проверку слияний.
    /// </summary>
    private void Start()
    {
      if (_grid == null)
      {
        Debug.LogError("Grid not set on GridObserver!");
        return;
      }

      // Предзагружаем соседей для всех клеток (ускоряет работу)
      foreach (var cell in _grid.Cells)
        _neighbors[cell] = GetNeighbors(cell);

      ScanAndRegisterStacks();

      // Асинхронно запускаем глобальную проверку всех стеков
      CheckAllStacksForMergesAsync().Forget();
    }

    /// <summary>
    /// Отслеживает отпускание мыши.
    /// Если появился новый стек — запускает проверку слияний и проверку поражения.
    /// </summary>
    private async UniTaskVoid Update()
    {
      if (Input.GetMouseButtonUp(0))
      {
        // Проверяем — появился ли новый стек на сетке
        if (RescanForNewStacks(out var newCell))
        {
          _lastAddedCell = newCell;

          // Запускаем слияние от этой клетки
          await ProcessMergesFromCellAsync(_lastAddedCell);

          // Проверка условий проигрыша
          await CheckForLoseConditionAsync();
        }
      }
    }

    /// <summary>
    /// Полное сканирование сетки, заносит все стеки в HashSet.
    /// </summary>
    private void ScanAndRegisterStacks()
    {
      _stacksOnGrid.Clear();

      foreach (var cell in _grid.Cells)
      {
        var stack = cell.GetComponentInChildren<Stack>();

        // Отсеиваем несуществующие и перетаскиваемые стеки
        if (stack != null && !stack.IsDragged)
          _stacksOnGrid.Add(stack);
      }

      CheckAllStacksForMergesAsync().Forget();
    }

    /// <summary>
    /// Определяет, появился ли новый стек после хода игрока.
    /// </summary>
    private bool RescanForNewStacks(out Cell newCell)
    {
      foreach (var cell in _grid.Cells)
      {
        var stack = cell.GetComponentInChildren<Stack>();
        if (stack == null || stack.IsDestroyed() || stack.IsDragged)
          continue;

        // Если этого стека ещё нет в HashSet — он новый
        if (_stacksOnGrid.Contains(stack))
          continue;

        _stacksOnGrid.Add(stack);
        newCell = cell;
        return true;
      }

      newCell = null;
      return false;
    }

    /// <summary>
    /// Проверяет ВСЕ стеки на сетке.
    /// Запускает слияния циклически, пока происходят изменения.
    /// </summary>
    private async UniTask CheckAllStacksForMergesAsync()
    {
      bool merged;

      do
      {
        merged = false;

        var stacks = _grid.Cells
          .Select(c => c.GetComponentInChildren<Stack>())
          .Where(s => s != null && !s.IsDragged)
          .ToList();

        foreach (var stack in stacks)
        {
          var cell = stack.Cell;
          if (cell == null || stack.IsDragged)
            continue;

          // Если слияние произошло — повторяем цикл
          if (await ProcessMergesFromCellAsync(cell, recursiveCheck: false))
            merged = true;
        }
      } while (merged);

      // После полной стабилизации — проверяем пороги цвета
      await CheckAllStacksForColorThresholdAsync();

      // И проверяем, не наступил ли проигрыш
      await CheckForLoseConditionAsync();
    }

    /// <summary>
    /// Выполняет цепочку слияний, начиная с одной клетки.
    /// Механика похожа на "поглощение" соседних тайлов одного цвета.
    /// </summary>
    private async UniTask<bool> ProcessMergesFromCellAsync(Cell centerCell, bool recursiveCheck = true)
    {
      if (centerCell == null)
        return false;

      var centerStack = centerCell.GetComponentInChildren<Stack>();
      if (centerStack == null || centerStack.IsDragged)
        return false;

      bool mergedAny = false;
      bool keepMerging;

      do
      {
        keepMerging = false;

        // Список соседних клеток, в которых есть стеки
        var neighborCells = _neighbors[centerCell]
          .Where(n =>
          {
            var stack = n.GetComponentInChildren<Stack>();
            return stack != null && !stack.IsDragged;
          })
          .ToList();

        if (neighborCells.Count == 0)
          break;

        // Цвет верхнего тайла в центральном стеке
        Color baseColor;

        try
        {
          baseColor = centerStack.GetLastCellColor();
        }
        catch
        {
          break;
        }

        // Соседи, у которых совпадают верхние цвета
        var sameColorNeighbors = neighborCells
          .Where(n =>
          {
            var s = n.GetComponentInChildren<Stack>();
            return s.GetLastCellColor() == baseColor;
          })
          .ToList();

        if (sameColorNeighbors.Count == 0)
          break;

        // Начинаем перенос тайлов одного цвета
        foreach (var neighbor in sameColorNeighbors)
        {
          var neighborStack = neighbor.GetComponentInChildren<Stack>();
          if (neighborStack == null || neighborStack.IsDragged)
            continue;

          var tilesToMove = GetCellsToMove(neighborStack, baseColor);
          if (tilesToMove.Count == 0)
            continue;

          // Перелёт тайлов в центральный стек
          await MoveCellsToOtherStackAsync(tilesToMove, centerStack);

          mergedAny = true;
          keepMerging = true;
        }

        // Проверяем порог заполнения
        if (!centerStack.IsDragged)
          await centerStack.CheckForColorThreshold();

        // Обновляем ссылку на центральный стек — вдруг он изменился
        centerStack = centerCell.GetComponentInChildren<Stack>();
      } while (keepMerging && centerStack != null && !centerStack.IsDragged && centerStack.Tiles.Count > 0);

      // После цепного слияния запускаем глобальную проверку
      if (mergedAny && recursiveCheck)
        await CheckAllStacksForMergesAsync();

      return mergedAny;
    }

    /// <summary>
    /// Отбирает тайлы сверху вниз, пока их цвет совпадает с переданным.
    /// </summary>
    private List<StackTile> GetCellsToMove(Stack stack, Color color)
    {
      var result = new List<StackTile>();
      if (stack == null || stack.Tiles == null || stack.IsDragged)
        return result;

      // Проходим по стеку сверху вниз
      for (int i = stack.Tiles.Count - 1; i >= 0; i--)
      {
        var go = stack.Tiles[i];
        if (go == null)
          break;

        var tile = go.GetComponent<StackTile>();

        // Цвет не совпал → прекращаем
        if (tile.Color != color)
          break;

        result.Add(tile);
      }

      return result;
    }

    /// <summary>
    /// Удаляет тайлы из одного стека, переносит в другой и запускает анимации.
    /// </summary>
    private async UniTask MoveCellsToOtherStackAsync(List<StackTile> cellsToMove, Stack targetStack)
    {
      if (cellsToMove == null || targetStack == null || targetStack.IsDragged)
        return;

      List<GameObject> movedTiles = new List<GameObject>();
      Vector3 moveDirection = Vector3.forward; // направление перелёта
      Stack prevStack = null;

      // Перебираем тайлы сверху вниз
      for (int i = cellsToMove.Count - 1; i >= 0; i--)
      {
        var tile = cellsToMove[i];
        if (tile == null)
          continue;

        prevStack = tile.GetComponentInParent<Stack>();
        if (prevStack == null || prevStack.IsDragged)
          continue;

        // Удаляем тайл из старого стека
        prevStack.Remove(tile.gameObject);

        // Если стек опустел — удаляем его
        if (prevStack.Tiles.Count == 0)
          RemoveStack(prevStack);

        // Вычисляем направление перелёта
        moveDirection = (targetStack.transform.position - prevStack.transform.position).normalized;

        movedTiles.Add(tile.gameObject);
      }

      await RecalcStackPositionsAsync(targetStack, movedTiles, moveDirection);
    }

    /// <summary>
    /// Полностью удаляет стек и освобождает клетку.
    /// </summary>
    private void RemoveStack(Stack stack)
    {
      if (stack == null)
        return;

      _stacksOnGrid.Remove(stack);

      var cell = stack.Cell;
      if (cell != null)
      {
        cell.SetEmpty(true);
        cell.ShineOff();
      }

      Destroy(stack.gameObject);
    }

    /// <summary>
    /// Запускает анимацию перелёта всех тайлов в целевой стек.
    /// </summary>
    private UniTask RecalcStackPositionsAsync(Stack stack, List<GameObject> movedTiles, Vector3 moveDirection)
    {
      var uts = new UniTaskCompletionSource();

      if (stack == null || movedTiles == null || movedTiles.Count == 0)
      {
        uts.TrySetResult();
        return uts.Task;
      }

      float delay = 0f;
      int completed = 0;
      int total = movedTiles.Count;

      for (int i = movedTiles.Count - 1; i >= 0; i--)
      {
        var go = movedTiles[i];
        if (go == null)
        {
          completed++;
          continue;
        }

        var tile = go.GetComponent<StackTile>();

        // Добавляем тайл в новый стек
        tile.SetParent(stack.transform);
        stack.Add(tile.gameObject);

        // Позиция тайла сверху стека
        Vector3 targetPosition = stack.transform.position +
                                 Vector3.up * (0.5f * stack.Tiles.IndexOf(go));

        // Траектория перелёта (дуга)
        Vector3 startPosition = go.transform.position;
        Vector3 aboveOldStack = startPosition + Vector3.up * 2f;
        Vector3 aboveNewStack = targetPosition + Vector3.up * 2f;

        Vector3[] path = new Vector3[]
        {
          startPosition,
          aboveOldStack,
          aboveNewStack,
          targetPosition
        };

        Quaternion prefabRotation = Quaternion.Euler(90f, 90f, 0f);
        Quaternion targetRotation =
          Quaternion.LookRotation(moveDirection) *
          Quaternion.Euler(270f, 90f, 0f);

        // Звук перелёта
        AudioFacade.Instance.PlaySort();

        // Анимация пути
        go.transform.DOPath(path, _moveDuration, PathType.CatmullRom)
          .SetDelay(delay)
          .SetEase(Ease.InOutSine);

        // Анимация вращения
        go.transform.DORotateQuaternion(targetRotation, _moveDuration)
          .SetDelay(delay)
          .SetEase(Ease.InOutSine)
          .OnComplete(() =>
          {
            go.transform.rotation = prefabRotation;

            completed++;
            if (completed >= total)
              uts.TrySetResult();
          });

        delay += _pauseBetween;
      }

      return uts.Task;
    }

    /// <summary>
    /// Проверяет все стеки на превышение порога одного цвета.
    /// </summary>
    private async UniTask CheckAllStacksForColorThresholdAsync()
    {
      foreach (var stack in _stacksOnGrid.ToList())
      {
        if (stack != null && !stack.IsDragged)
          await stack.CheckForColorThreshold();
      }
    }

    /// <summary>
    /// Находит всех соседей клетки на слое клеток через OverlapSphere.
    /// </summary>
    private List<Cell> GetNeighbors(Cell cell)
    {
      if (cell == null)
        return new List<Cell>();

      LayerMask mask = 1 << cell.gameObject.layer;

      var hits = Physics.OverlapSphere(cell.transform.position, 5f, mask);

      return hits
        .Select(h => h.GetComponent<Cell>())
        .Where(c => c != null && c != cell)
        .ToList();
    }

    /// <summary>
    /// Проверка проигрыша:
    /// — Все клетки заполнены.
    /// — Нет соседей с одинаковыми цветами.
    /// </summary>
    private async UniTask CheckForLoseConditionAsync()
    {
      bool allFilled = _grid.Cells.All(c => !c.IsEmpty);

      if (!allFilled)
        return;

      // Проверяем, есть ли хотя бы одно возможное слияние
      foreach (var cell in _grid.Cells)
      {
        var stack = cell.GetComponentInChildren<Stack>();
        if (stack == null)
          continue;

        Color color;

        try
        {
          color = stack.GetLastCellColor();
        }
        catch
        {
          continue;
        }

        foreach (var neighbor in _neighbors[cell])
        {
          var neighborStack = neighbor.GetComponentInChildren<Stack>();
          if (neighborStack == null)
            continue;

          if (neighborStack.GetLastCellColor() == color)
            return; // ход ещё возможен
        }
      }

      // Ходов нет → проигрыш
      await ShowLosePopupAsync();
    }

    /// <summary>
    /// Показывает окно поражения.
    /// </summary>
    private UniTask ShowLosePopupAsync()
    {
      _mainMenu.Open();
      return UniTask.CompletedTask;
    }

    /// <summary>
    /// Удаляет стек из HashSet по использованию бустера разрушения.
    /// </summary>
    public void RemoveStackFromCellByBooster(Cell cell)
    {
      _stacksOnGrid.Remove(cell.GetComponentInChildren<Stack>());
    }
  }
}