using System.Collections.Generic;
using UnityEngine;

public class EnvironmentalDamage : IDamageSource
{
  public EnvironmentTile tileType;

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

  // No partial damage from environmental hazards; resistances can ignore altogether
  public float CalculateDamageAfterResistances(Character c)
  {
    if (
      c.GetDamageTypeResistanceLevel(damageType) >= GetResistanceRequiredForImmunity() // high enough resistance to ignore altogether
      )
    {
      return 0;
    }
    else
    {
      return damageAmount;
    }
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

  public List<CharacterMovementAbility> movementAbilitiesWhichBypassDamage
  {
    get
    {
      return tileType.movementAbilitiesWhichBypassDamage;
    }
  }

  public float stunMagnitude
  {
    get
    {
      return tileType.environmentalDamageInfo.stun;
    }
  }

  public bool isNonlethal 
  {
    get 
    {
      return tileType.environmentalDamageInfo.isNonlethal;
    }
  }

  public bool isCritAttack 
  {
    get 
    {
      return tileType.environmentalDamageInfo.isCritAttack;
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