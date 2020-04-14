using System;
using UnityEngine;

public class Utils
{
  public static float EaseOut(float t, float p)
  {
    return (float)(1f - Math.Pow(1 - t, p));
  }

}