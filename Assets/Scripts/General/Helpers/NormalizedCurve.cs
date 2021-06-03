
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NormalizedCurve
{
  public Overrideable<AnimationCurve> curve;
  public Overrideable<float> magnitude;

  public float Evaluate(Character c, float t)
  {
    return curve.Resolve(c).Evaluate(t) * magnitude.Resolve(c);
  }
}