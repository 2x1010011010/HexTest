using System;
using System.Collections.Generic;
using UnityEngine;

namespace HexaSortTest.CodeBase.GameLogic.Meta
{
  [Serializable]
  public class MetaTileObjectGroup
  {
    [SerializeField] private List<GameObject> _objects = new();

    public List<GameObject> Objects => _objects;
  }
}