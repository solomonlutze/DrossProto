
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NormalizedCurve
{
  public Overrideable<AnimationCurve> curve;
  public Overrideable<float> magnitude;

  public float Evaluate(Character c, float t)
  {
    Debug.Log("magnitude evaluated to " + magnitude.Resolve(c));
    return curve.Resolve(c).Evaluate(t) * magnitude.Resolve(c);
  }

  public float EvaluateIncrement(Character c, float tCurrent, float tPrevious)
  {
    Debug.Log("magnitude evaluated to " + magnitude.Resolve(c));
    return (curve.Resolve(c).Evaluate(tCurrent) - curve.Resolve(c).Evaluate(tPrevious)) * magnitude.Resolve(c);
  }
}