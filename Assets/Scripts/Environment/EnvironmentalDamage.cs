
using System;
using UnityEngine;

public class EnvironmentalDamage : IDamageSource
{
  public EnvironmentTile tileType;
  public Character character;

  public string sourceString
  {
    get
    {
      return tileType.name;
    }
  }
  public void Init(EnvironmentTile t)
  {
    tileType = t;
  }


  public bool IsOwnedBy(Character c)
  {
    return false;
  }

  public bool IsSameOwnerType(Character c)
  {
    return false;
  }

  public int damageAmount
  {
    get
    {
      return tileType.environmentalDamageInfo.damageAmount;
    }
  }

  public DamageType damageType
  {
    get
    {
      return tileType.environmentalDamageInfo.damageType;
    }
  }

  public float stunMagnitude
  {
    get
    {
      return tileType.environmentalDamageInfo.stun;
    }
  }

  public bool ignoresInvulnerability
  {
    get
    {
      return tileType.environmentalDamageInfo.ignoreInvulnerability;
    }
  }

  public float invulnerabilityWindow
  {
    get
    {
      return tileType.environmentalDamageInfo.invulnerabilityWindow;
    }
  }
  public Vector3 GetKnockbackForCharacter(Character c)
  {
    return Vector3.zero;
  }
  public int GetResistanceRequiredForImmunity()
  {
    return tileType.environmentalDamageInfo.resistanceRequiredForImmunity;
  }

  public bool forcesItemDrop
  {
    get
    {
      return false;
    }
  }
}