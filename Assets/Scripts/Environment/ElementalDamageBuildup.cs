using UnityEngine;
public class ElementalDamageBuildup
{
  public float timeElapsed;
  public float remainingMagnitude;
}

[System.Serializable]
public class ElementalBuildupConstant
{
  public float delay;
  public float dps;
  public float staminaDps;
}