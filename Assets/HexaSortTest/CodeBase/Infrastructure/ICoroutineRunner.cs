using System.Collections;
using UnityEngine;

namespace HexaSortTest.CodeBase.Infrastructure
{
  public interface ICoroutineRunner
  {
    Coroutine StartCoroutine(IEnumerator routine);
  }
}