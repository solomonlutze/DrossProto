using System;
using System.Collections;
using UnityEngine;

public class Damage
{
  DamageData damageData;
  CharacterAttackModifiers attackModifiers;

  public Character owningCharacter;
  private Hitbox owningHitbox;
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
  public Damage(DamageData damage, CharacterAttackModifiers mods)
  {
    damageData = damage;
    attackModifiers = mods;

  }

  // For attacks
  public Damage(DamageData damage, CharacterAttackModifiers mods, Character character, Hitbox hitbox)
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