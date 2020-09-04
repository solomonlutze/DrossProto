
using System;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour, IDamageSource
{
  public AttackSkillEffect attackSkillEffect;
  public AttackSkillData attack_old;
  public Character owner;

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

  public void Init(AttackSkillData atk, Character ch)
  {
    attack_old = atk;
    damageInfo = attack_old.baseDamage;
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
      return damageInfo.damageAmount;
      // return attack_old.GetDamageAmount(character);
    }
  }

  public DamageType damageType
  {
    get
    {
      return damageInfo.damageType;
      // return attack_old.GetDamageType(character);
    }
  }

  public float stunMagnitude
  {
    get
    {
      return damageInfo.stun;
      // return attack_old.GetStun(character);
    }
  }

  public bool ignoresInvulnerability
  {
    get
    {
      return damageInfo.ignoreInvulnerability;
      // return attack_old.IgnoresInvulnerability();
    }
  }

  public float invulnerabilityWindow
  {
    get
    {
      return damageInfo.invulnerabilityWindow;
      return attack_old.GetInvulnerabilityWindow();
    }
  }

  public Vector3 GetKnockbackForCharacter(Character c)
  {
    Debug.Log("damageInfo: " + damageInfo);
    Debug.Log("transform: " + transform);
    Debug.Log("owner: " + owner);
    return damageInfo.knockback
      * (transform.position - owner.transform.position);
    // return attack_old.GetKnockback(character, this);
  }

  public bool forcesItemDrop
  {
    get
    {
      return attack_old.ForcesItemDrop();
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