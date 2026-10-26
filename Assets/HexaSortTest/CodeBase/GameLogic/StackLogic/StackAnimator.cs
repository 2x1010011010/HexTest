using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using HexaSortTest.CodeBase.GameLogic.GridLogic;
using HexaSortTest.CodeBase.GameLogic.SoundLogic;
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
    [SerializeField, Range(0.1f, 1f), BoxGroup("MOVE ANIMATION")] private float _durationCent = 0.9f;
    [SerializeField, BoxGroup("MOVE ANIMATION")] private float _curveHeight = 6f;
    [SerializeField, BoxGroup("STACK PARAMETERS")] private float _verticalShift = 0.7f;

    public async UniTask DestroyTilesAsync(List<StackTile> tiles, Stack stack, CancellationToken token)
    {
      if (tiles.Count == 0) return;
      var scales = new List<Vector3>();
      var sequence = DOTween.Sequence();
      for (int i = 0; i < tiles.Count; i++)
      {
        var tile = tiles[i];
        scales.Add(tile.transform.localScale);
        sequence.Insert(i * _pauseBetween, tile.transform.DOScale(Vector3.zero, _scaleDuration)
          .SetEase(Ease.InOutSine).OnStart(() => AudioFacade.Instance?.PlayClose()));
      }
      await BoardTween.PlayAsync(sequence, token);
      // Mutate the model only once the whole collection animation has finished.
      for (int i = 0; i < tiles.Count; i++)
      {
        var tile = tiles[i];
        stack.Remove(tile.gameObject);
        tile.transform.localScale = scales[i];
        tile.Color = Color.white;
        if (stack.PoolInstance != null) stack.PoolInstance.ReturnObject(tile);
        else Destroy(tile.gameObject);
      }
    }

    public async UniTask MoveStackTilesAsync(Stack target, List<GameObject> movedTiles, Vector3 direction, CancellationToken token)
    {
      if (movedTiles.Count == 0) return;
      var sequence = DOTween.Sequence();
      Quaternion restRotation = target.transform.rotation * Quaternion.Euler(0f, 90f, 0f);
      Vector3 flipAxis = Vector3.Cross(-Vector3.up, direction).normalized;
      if (flipAxis.sqrMagnitude < 0.001f) flipAxis = Vector3.right;
      for (int i = 0; i < movedTiles.Count; i++)
      {
        var go = movedTiles[i];
        var tile = go.GetComponent<StackTile>();
        tile.SetParent(target.transform);
        target.Add(go);
        var destination = target.transform.position + Vector3.up * (_verticalShift * (target.Tiles.Count - 1));
        var path = new[] { destination, go.transform.position + Vector3.up * _curveHeight, destination + Vector3.up * _curveHeight };
        float delay = i * _movePauseBetween;
        sequence.Insert(delay, go.transform.DOPath(path, _moveDuration, PathType.CubicBezier).SetEase(Ease.InOutSine));
        sequence.Insert(delay, go.transform.DORotateQuaternion(Quaternion.AngleAxis(180f, flipAxis) * restRotation,
          _moveDuration * _durationCent).SetEase(Ease.InOutSine));
      }
      // A sequence waits for BOTH translation and rotation, including the last delayed tile.
      await BoardTween.PlayAsync(sequence, token);
      foreach (var go in movedTiles) go.transform.rotation = restRotation;
      AudioFacade.Instance?.PlaySort();
    }
  }
}
