using UnityEngine;

public class CharacterHelperClasses { }


public enum CharacterType
{
  Player,
  Enemy
}
// non-derivable values like health, current number of molts, etc
public enum CharacterVital
{
  CurrentHealth,
  CurrentEnvironmentalDamageCooldown,
  CurrentStamina,
  CurrentCarapace, // Carapace might be "balance" sometime
  CurrentMaxHealth,
  CurrentMoltCount
}

// values driving physical character behavior
// base values live in character data
// can include maximums, cooldown periods, etc
// not usually user facing, so can be pretty grody and granular if need be
public enum CharacterStat
{
  REMOVE_0 = 0,
  MaxHealthLostPerMolt = 1,
  DetectableRange = 2,
  MoveAcceleration = 3,
  FlightAcceleration = 4,
  Stamina = 5,
  DashDistance = 6,
  DashDuration = 7,
  DashRecoveryDuration = 8,
  RotationSpeed = 9,
  Carapace = 10,
  MoltDuration = 11,
  BlockingMoveAcceleration = 12,
  BlockingRotationSpeed = 13,
  MaxHealth = 14
}

public enum CharacterAttribute
{
  REMOVE_0 = 0,
  REMOVE_1 = 1,
  REMOVE_2 = 2,
  Burrow = 3,
  Camouflage = 4,
  HazardResistance = 5,
  Resist_Fungal = 6,
  Resist_Heat = 7,
  Resist_Acid = 8,
  Resist_Physical = 9,
  WaterResistance = 10,
  Flight = 11,
  Dash = 12,
  Health = 13,
  Metabolism = 14,
  SightRange = 15,
  DarkVision = 16,
  MoltingEfficiency = 17,
  Reflexes = 18,
  AntennaeSensitivity = 19
}

public enum CharacterAttackValue
{
  Damage,
  Range,
  HitboxSize,
  Knockback,
  Stun,
  AttackSpeed,
  Cooldown,
  DurabilityDamage,
  Venom,
  AcidDamage,
}

// Special modes of character movement.
// Possibly unnecessary!!
public enum CharacterMovementAbility
{
  Burrow,
  FastFeet,
  Halteres,
  Hover,
  StickyFeet,
  WaterStride
}

public enum AscendingDescendingState
{ // negative is up and positive is down because the map is backwards, Do Not @ Me
  Ascending = -1,
  Descending = 1,
  None = 0
}

public enum CharacterPerceptionAbility
{
  SensitiveAntennae
}

[System.Serializable]
public class CharacterAttributeModification
{
  public CharacterAttribute statToModify;

  public int magnitude;

  public float applicationDuration;
  public float duration;
  public float delay;

  public string source;

  public CharacterAttributeModification(CharacterAttribute s, int m, float dur, float del, string src)
  {
    statToModify = s;
    magnitude = m;
    duration = dur;
    delay = del;
    source = src;
  }
}

[System.Serializable]
public class ActiveStatModification
{
  public int magnitude;
  public string source;

  public ActiveStatModification(int m, string s)
  {
    magnitude = m;
    source = s;
  }
}

[System.Serializable]
public class CharacterAttackModifiers
{
  public CharacterAttackValueToIntDictionary attackValueModifiers;
  public bool forcesLymphDrop;

  public CharacterAttackModifiers()
  {
    attackValueModifiers = new CharacterAttackValueToIntDictionary();
  }

}

[System.Serializable]
public class TraitsLoadout
{
  public Trait head;
  public Trait thorax;
  public Trait abdomen;
  public Trait legs;
  public Trait wings;

  public bool AllTraitsEmpty()
  {
    return head == null
        && thorax == null
        && abdomen == null
        && legs == null
        && wings == null;
  }
}
[System.Serializable]
public class PartStatusInfo
{
  // "Stamina" might be branded as "exhaustion" in game!
  public float currentStamina = 0;
  public float currentHealth = 100;
  public float recoveryCooldown = 0;
  public float remainingStamina
  {
    get
    {
      return currentHealth - currentStamina;
    }
  }

  public bool HasStaminaRemaining()
  {
    return currentStamina < currentHealth;
  }
  // public float GetRemainingStamina()
  // {
  //   return currentHealth - currentStamina;
  // }

  public void AdjustCurrentStamina(float adjustment)
  {
    float staminaAdjustment = adjustment;
    if (Mathf.RoundToInt(adjustment) > 0 && currentStamina != 0)
    {
      Debug.Log("    Stamina Damage - original " + currentStamina + ", currentHealth " + currentHealth + ", remainingStamina " + remainingStamina + ", adjustment " + adjustment);
    }
    if (Mathf.RoundToInt(adjustment) != 0 && adjustment > remainingStamina)
    { // taking more stamina damage than we have health; deal some as part damage
      staminaAdjustment -= remainingStamina;
      float partDamage = Mathf.Clamp(adjustment - remainingStamina, 0, adjustment);
      partDamage = partDamage * GameMaster.Instance.settingsData.exhaustionDamageMultiplier;
      AdjustCurrentHealth(-partDamage);
    }
    currentStamina = Mathf.Clamp(currentStamina + adjustment, 0, currentHealth);
  }

  public float AdjustCurrentHealth(float adjustment, bool isNonbreaking = false)
  {
    float originalHealth = currentHealth;
    currentHealth = Mathf.Clamp(currentHealth + adjustment, isNonbreaking ? 1 : 0, 100);
    AdjustCurrentStamina(0);
    Debug.Log("   Part Damage - original " + originalHealth + ", adjustment " + adjustment + ", new current " + currentHealth);
    return adjustment + originalHealth - currentHealth; // if part breaks, return the amount of surplus damage not applied; should this just damage other parts directly, and/or should the damage get eaten by the broken part?
  }
}