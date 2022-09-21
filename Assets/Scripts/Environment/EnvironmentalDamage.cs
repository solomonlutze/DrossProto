using System.Collections.Generic;
using UnityEngine;

public enum EnvironmentalDamageSourceStatus { Active, Warmup, Inactive }
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

  public bool IsEnvironmentalDamageSourceWarmup()
  {
    return GetEnvironmentalDamageSourceStatus() == EnvironmentalDamageSourceStatus.Warmup;
  }

  public bool IsEnvironmentalDamageSourceActive()
  {
    return GetEnvironmentalDamageSourceStatus() == EnvironmentalDamageSourceStatus.Active;
  }

  public EnvironmentalDamageSourceStatus GetEnvironmentalDamageSourceStatus()
  {
    if (!tileType.isDamagePeriodic) { return EnvironmentalDamageSourceStatus.Active; }
    float modTimeSinceStartup = GameMaster.Instance.timeSinceStartup.Elapsed.Seconds % tileType.totalPeriodicTime;
    if (modTimeSinceStartup < tileType.periodicInactiveTime) { return EnvironmentalDamageSourceStatus.Inactive; }
    if (modTimeSinceStartup < tileType.periodicInactiveTime + tileType.periodicWarmupTime) { return EnvironmentalDamageSourceStatus.Warmup; }
    return EnvironmentalDamageSourceStatus.Active;
  }

  public float CalculateDamageAfterResistances(Character c)
  {
    if (c != null)
    {
      Debug.Log("damage amount " + damageAmount + " adjusted to " + damageAmount * c.GetDamageMultiplier(damageType));
      return damageAmount * c.GetDamageMultiplier(damageType);
    }
    return damageAmount;
    // if (
    //   c.GetDamageTypeResistanceLevel(damageType) >= GetResistanceRequiredForImmunity() // high enough resistance to ignore altogether
    //   )
    // {
    //   return 0;
    // }
    // else
    // {
    //   return damageAmount;
    // }
  }
  public float CalculateStaminaDamageAfterResistances(Character c)
  {
    if (c != null)
    {
      Debug.Log("damage amount " + staminaDamageAmount + " adjusted to " + staminaDamageAmount * c.GetDamageMultiplier(damageType));
      return staminaDamageAmount * c.GetDamageMultiplier(damageType);
    }
    return staminaDamageAmount;
    // if (
    //   c.GetDamageTypeResistanceLevel(damageType) >= GetResistanceRequiredForImmunity() // high enough resistance to ignore altogether
    //   )
    // {
    //   return 0;
    // }
    // else
    // {
    //   return damageAmount;
    // }
  }

  public int damageAmount
  {
    get
    {
      return Mathf.FloorToInt(tileType.environmentalDamageInfo.damageAmount.Resolve() * GameMaster.Instance.DEBUG_damageMultiplier);
    }
  }

  public int staminaDamageAmount
  {
    get
    {
      return Mathf.FloorToInt(tileType.environmentalDamageInfo.staminaDamageAmount.Resolve() * GameMaster.Instance.DEBUG_damageMultiplier);
    }
  }

  public DamageType damageType
  {
    get
    {
      return tileType.environmentalDamageInfo.damageType.Resolve();
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
      return tileType.environmentalDamageInfo.stun.Resolve();
    }
  }

  public bool isNonlethal
  {
    get
    {
      return tileType.environmentalDamageInfo.isNonlethal.Resolve();
    }
  }

  public bool ignoresInvulnerability
  {
    get
    {
      return tileType.environmentalDamageInfo.ignoreInvulnerability.Resolve();
    }
  }


  public bool GetCharacterWithinVerticalRange(Character c)
  {
    if (tileType.floorTilemapType == FloorTilemapType.Object)
    {
      return true;
    }
    return !c.IsMidair(); // TODO: FIXME!!!
  }

  public float invulnerabilityWindow
  {
    get
    {
      return tileType.environmentalDamageInfo.invulnerabilityWindow.Resolve();
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
  public bool applySlowdown
  {
    get
    {
      return false;
    }
  }

}