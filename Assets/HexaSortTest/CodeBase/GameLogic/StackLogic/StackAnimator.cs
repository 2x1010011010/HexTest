using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;
using HexaSortTest.CodeBase.GameLogic.UI.HUD;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.StackLogic
{
  public class StackAnimator : MonoBehaviour
  {
    [SerializeField, BoxGroup("SCALE")] private float _scaleDuration = 0.05f;
    [SerializeField, BoxGroup("SCALE")] private float _pauseBetween = 0.05f;

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
  }
}