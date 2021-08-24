
using System;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour, IDamageSource
{
  AttackSkillEffect attackSkillEffect;
  Character owner;

  protected DamageInfo damageInfo;
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

  public void Init(Character ch, DamageInfo di)
  {
    owner = ch;
    damageInfo = di;
  }

  public void Init(Character ch)
  {
    owner = ch;
  }


  private void OnTriggerEnter2D(Collider2D other)
  {
    other.gameObject.SendMessage("TakeDamage", this, SendMessageOptions.DontRequireReceiver);
  }

  public bool IsOwnedBy(Character c)
  {
    return c == owner;
  }

  public bool IsSameOwnerType(Character c)
  {
    return owner && owner.characterType == c.characterType;
  }

  public int damageAmount
  {
    get
    {
      return damageInfo.damageAmount.Resolve(owner);
      // return attack_old.GetDamageAmount(character);
    }
  }

  public DamageType damageType
  {
    get
    {
      return damageInfo.damageType.Resolve(owner);
      // return attack_old.GetDamageType(character);
    }
  }

  public float stunMagnitude
  {
    get
    {
      return damageInfo.stun.Resolve(owner);
      // return attack_old.GetStun(character);
    }
  }

  public bool ignoresInvulnerability
  {
    get
    {
      return damageInfo.ignoreInvulnerability.Resolve(owner);
      // return attack_old.IgnoresInvulnerability();
    }
  }

  public bool isNonlethal
  {
    get
    {
      return damageInfo.isNonlethal.Resolve(owner);
    }
  }

  public float invulnerabilityWindow
  {
    get
    {
      return damageInfo.invulnerabilityWindow.Resolve(owner);
    }
  }

  public Vector3 GetKnockbackForCharacter(Character c)
  {
    return damageInfo.knockback.Resolve(owner)
      * ((transform.position - owner.transform.position).normalized);
    // return attack_old.GetKnockback(character, this);
  }

  public float CalculateDamageAfterResistances(Character c)
  {
    if (c != null && c.blocking)
    {
      return ((1 - c.GetDamageReductionPercent(damageType) / 100f) * damageAmount);
    }
    return damageAmount;
  }

  public List<CharacterMovementAbility> movementAbilitiesWhichBypassDamage
  {
    get
    {
      return new List<CharacterMovementAbility>();
    }
  }

}