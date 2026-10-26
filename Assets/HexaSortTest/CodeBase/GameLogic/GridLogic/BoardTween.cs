using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace HexaSortTest.CodeBase.GameLogic.GridLogic
{
  internal static class BoardTween
  {
    public static async UniTask PlayAsync(Tween tween, CancellationToken token)
    {
      tween.SetAutoKill(false);
      try
      {
        token.ThrowIfCancellationRequested();
        await UniTask.WaitUntil(() => !tween.IsActive() || tween.IsComplete(), cancellationToken: token);
        token.ThrowIfCancellationRequested();
        if (!tween.IsActive()) throw new OperationCanceledException("Board tween was interrupted.");
      }
      finally
      {
        if (tween.IsActive()) tween.Kill(false);
      }
    }
  }
}
