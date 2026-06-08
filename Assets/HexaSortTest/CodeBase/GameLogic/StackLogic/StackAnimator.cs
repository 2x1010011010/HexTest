using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;
using HexaSortTest.CodeBase.GameLogic.UI.HUD;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.StackLogic
{
  public class StackAnimator : MonoBehaviour
  {
    [SerializeField, BoxGroup("DESTROY ANIMATION")] private float _scaleDuration = 0.2f;
    [SerializeField, BoxGroup("DESTROY ANIMATION")] private float _pauseBetween = 0.2f;

    [SerializeField, BoxGroup("MOVE ANIMATION")] private float _movePauseBetween = 0.2f;
    [SerializeField, BoxGroup("MOVE ANIMATION")] private float _moveDuration = 0.4f;

    public async Task DestroyTilesAnimation(List<StackTile> tiles, Stack stack, int tilesCount)
    {
      if (tiles == null || tiles.Count == 0)
        return;

      int remaining = 0;
      foreach (var t in tiles)
        if (t != null)
          remaining++;

      if (remaining == 0)
        return;

      var tcs = new TaskCompletionSource<bool>();
      float delay = 0f;

      foreach (var cell in tiles)
      {
        if (cell == null)
        {
          remaining--;
          if (remaining == 0)
            tcs.TrySetResult(true);
          continue;
        }

        stack.Remove(cell.gameObject);

        var baseScale = cell.transform.localScale;
        cell.transform.DOScale(Vector3.zero, _scaleDuration)
          .SetDelay(delay)
          .SetEase(Ease.InOutSine)
          .OnStart(() =>
          {
            AudioFacade.Instance.PlayClose();
            HudObserver.Instance.AddTiles(tilesCount);
          })
          .OnComplete(() =>
          {
            cell.SetActive(false);
            cell.transform.localScale = baseScale;
            cell.transform.position = Vector3.zero;
            cell.Color = Color.white;
            stack.PoolInstance?.ReturnObject(cell);

            remaining--;
            if (remaining == 0)
              tcs.TrySetResult(true);
          });

        delay += _pauseBetween;
      }

      await tcs.Task;
    }

    public UniTask MoveStackTilesAnimation(Stack targetStack, List<GameObject> movedTiles, Vector3 moveDirection)
    {
      var uts = new UniTaskCompletionSource();

      if (targetStack == null || movedTiles == null || movedTiles.Count == 0)
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
          if (completed >= total)
            uts.TrySetResult();
          continue;
        }

        var tile = go.GetComponent<StackTile>();

        tile.SetParent(targetStack.transform);
        targetStack.Add(tile.gameObject);

        Vector3 targetPosition = targetStack.transform.position +
                                 Vector3.up * (0.5f * targetStack.Tiles.IndexOf(go));

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

        Quaternion prefabRotation = Quaternion.Euler(270f, 90f, 0f);
        Vector3 flipAxis = Vector3.Cross(-Vector3.up, moveDirection).normalized;
        Quaternion targetRotation = Quaternion.AngleAxis(180f, flipAxis) * prefabRotation;

        go.transform.DOPath(path, _moveDuration, PathType.CatmullRom)
          .SetDelay(delay)
          .SetEase(Ease.InOutSine);

        go.transform.DOLocalRotateQuaternion(targetRotation, _moveDuration)
          .SetDelay(delay)
          .SetEase(Ease.InOutSine)
          .OnComplete(() =>
          {
            AudioFacade.Instance.PlaySort();
            go.transform.rotation = prefabRotation;

            completed++;
            if (completed >= total)
              uts.TrySetResult();
          });

        delay += _movePauseBetween;
      }

      return uts.Task;
    }
  }
}