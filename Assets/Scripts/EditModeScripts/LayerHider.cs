﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LayerHider : MonoBehaviour
{
  bool inEditMode = false;

#if UNITY_EDITOR
  void Update()
  {
    if (PlayStateNotifier.inEditMode != inEditMode)
    {
      inEditMode = PlayStateNotifier.inEditMode;
      Debug.Log("inEditMode: " + inEditMode);
    }
  }
#endif
}
