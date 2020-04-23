
using System;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour, IDamageSource
{
  public CharacterAttackData attack;
  public Character character;

  protected string _sourceString = "";
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

  public void Init(CharacterAttackData atk, Character ch)
  {
    attack = atk;
    character = ch;
  }


  private void OnTriggerEnter2D(Collider2D other)
  {
    other.gameObject.SendMessage("TakeDamage", this, SendMessageOptions.DontRequireReceiver);
  }

  public bool IsOwnedBy(Character c)
  {
    return c == character;
  }

  public bool IsSameOwnerType(Character c)
  {
    return character && character.characterType == c.characterType;
  }

  public int damageAmount
  {
    get
    {
      return attack.GetDamageAmount(character);
    }
  }

  public DamageType damageType
  {
    get
    {
      return attack.GetDamageType(character);
    }
  }

  public float stunMagnitude
  {
    get
    {
      return attack.GetStun(character);
    }
  }

  public bool ignoresInvulnerability
  {
    get
    {
      return attack.IgnoresInvulnerability();
    }
  }

  public float invulnerabilityWindow
  {
    get
    {
      return attack.GetInvulnerabilityWindow();
    }
  }

  public Vector3 GetKnockbackForCharacter(Character c)
  {
    return attack.GetKnockback(character, this);
  }

  public bool forcesItemDrop
  {
    get
    {
      return attack.ForcesItemDrop();
    }
  }

  public float CalculateDamageAfterResistances(Character c)
  {
    if (c != null)
    {
      return ((1 - GetDamageTypeResistancePercent(c) / 100) * damageAmount);
    }
    return 0;
  }

  protected float GetDamageTypeResistancePercent(Character c)
  {
    return 34 * c.GetDamageTypeResistanceLevel(damageType); // TODO: get rid of magic number!! build it into resistance tiers maybe?
  }

  public List<CharacterMovementAbility> movementAbilitiesWhichBypassDamage
  {
    get
    {
      return new List<CharacterMovementAbility>();
    }
  }

}