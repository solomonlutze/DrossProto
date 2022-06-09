using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;


[System.Serializable]
public class DamageInfo
{
  public Overrideable<int> damageAmount;
  // amount of impulse force applied on hit
  public Overrideable<DamageType> damageType;

  public Overrideable<float> knockback;
  // Duration in seconds of stun. TODO: should this value represent something else?
  public Overrideable<float> stun;


  [Tooltip("Minimum time before this damage source can damage the same target again")]
  public Overrideable<float> invulnerabilityWindow;

  [Tooltip("Will not reduce an enemy below 1 health")]
  public Overrideable<bool> isNonlethal;
  // whether this damage object should respect invulnerability from OTHER targets
  public Overrideable<bool> ignoreInvulnerability;

  [Tooltip("How far up (x) and down (y) the attack hits, in normalized Z units")]
  public Overrideable<Vector2> verticalRange = new Overrideable<Vector2>(new Vector2(.1f, .2f));

}

[System.Serializable]
public class EnvironmentalDamageInfo : DamageInfo
{
  public int resistanceRequiredForImmunity;
}

// Damage instance that lives on hitboxes.
[System.Serializable]
public class Damage
{
  DamageInfo baseDamage;
  Character owner;
  private string _sourceString = "";
  public string sourceString
  {
    get
    {
      if (_sourceString == "")
      {
        _sourceString = Guid.NewGuid().ToString("N").Substring(0, 15);
      }
      return _sourceString;
    }
  }

}
