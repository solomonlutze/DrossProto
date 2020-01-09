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
  // amount of invulnerability this attack imparts. TODO: separate windows per damage source?
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
// Damage instance that lives on hitboxes.
[System.Serializable]
public class Damage
{
  DamageInfo baseDamage;
  CharacterAttack characterAttack;
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
public class Damage_OLD
{
  DamageData_OLD damageData;
  CharacterAttackModifiers attackModifiers;

  public Character owningCharacter;
  private Hitbox_OLD owningHitbox;
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

  // For environmental damage
  public Damage_OLD(DamageData_OLD damage, CharacterAttackModifiers mods)
  {
    damageData = damage;
    attackModifiers = mods;

  }

  // For attacks
  public Damage_OLD(DamageData_OLD damage, CharacterAttackModifiers mods, Character character, Hitbox_OLD hitbox)
  {
    damageData = damage;
    attackModifiers = mods;
    owningCharacter = character;
    owningHitbox = hitbox;
  }

  public float GetDamage()
  {
    return damageData.damageAmount + Character.GetAttackValueModifier(attackModifiers.attackValueModifiers, CharacterAttackValue.Damage);
  }

  public float GetKnockback()
  {
    return damageData.knockback + Character.GetAttackValueModifier(attackModifiers.attackValueModifiers, CharacterAttackValue.Knockback);
  }

  public Vector3 CalculateAndReturnKnockback()
  {
    if (owningCharacter == null || owningHitbox == null)
    {
      return Vector3.zero;
    }
    return (damageData.knockback + Character.GetAttackValueModifier(attackModifiers.attackValueModifiers, CharacterAttackValue.Knockback))
      * (owningCharacter.transform.position - owningHitbox.transform.position);
  }

  public float GetStun()
  {
    return damageData.stun + Character.GetAttackValueModifier(attackModifiers.attackValueModifiers, CharacterAttackValue.Stun);
  }

  public TileDurability GetDurabilityDamageLevel()
  {
    float rawNewDurability = Mathf.Max((float)damageData.durabilityDamageLevel, Character.GetAttackValueModifier(attackModifiers.attackValueModifiers, CharacterAttackValue.DurabilityDamage));
    return (TileDurability)rawNewDurability;
  }
  public bool IsCorrosive()
  {
    return Character.GetAttackValueModifier(attackModifiers.attackValueModifiers, CharacterAttackValue.AcidDamage) > 0;
  }

  public float GetInvulnerabilityWindow()
  {
    return damageData.invulnerabilityWindow;
  }
  public bool ForcesItemDrop()
  {
    return attackModifiers.forcesLymphDrop;
  }
  public bool IgnoresInvulnerability()
  {
    return damageData.ignoreInvulnerability;
  }
  public DamageType GetDamageType()
  {
    return damageData.damageType;
  }
}