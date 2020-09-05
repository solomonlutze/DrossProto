using System;
using System.Collections;
using UnityEngine;


[System.Serializable]
public class DamageInfo
{
  public int damageAmount;
  // amount of impulse force applied on hit
  public DamageType damageType;

  public float knockback;
  // Duration in seconds of stun. TODO: should this value represent something else?
  public float stun;
  // amount of invulnerability this attack imparts. 
  public float invulnerabilityWindow;
  // whether this damage object should respect invulnerability from OTHER targets
  public bool ignoreInvulnerability;
  // whether this damage object should corrode certain tiles
  // TODO: Possibly deprecated by acid damage
  public bool corrosive = false;

  // whether this damage object should force hit enemies to drop their items
  public bool forcesItemDrop = false;
  // TODO: possibly deprecated by attack stats
  public TileDurability durabilityDamageLevel = TileDurability.Delicate;

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
