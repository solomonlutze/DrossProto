
using System;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour, IDamageSource
{
  SkillEffect attackSkillEffect;
  Weapon owningWeapon;
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

  public void Init(Character ch, DamageInfo di, Weapon ow)
  {
    owner = ch;
    damageInfo = di;
    owningWeapon = ow;
  }

  public void Init(Character ch)
  {
    owner = ch;
  }


  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.gameObject.GetComponent<Character>()) // Hacky shit! Need a way to confirm damage dealt to anything, not just character.
    {
      other.gameObject.SendMessage("TakeDamage", this, SendMessageOptions.DontRequireReceiver);
      owner.OnWeaponHit(attackSkillEffect, other);
      owningWeapon.OnContact(other);
    }
    // Debug.Break();
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
      * ((c.transform.position - owner.transform.position).normalized);
  }

  // example: suppose weapon z is 0, vert range is (.5, 1)
  // remember: negative numbers are higher elevation on z axis, because fuck you
  public bool GetCharacterWithinVerticalRange(Character c)
  {
    float buffer = .00001f;
    Vector2 verticalRange = damageInfo.verticalRange.Resolve(owner);
    float maxHeight = transform.position.z - (verticalRange.x * GridConstants.Z_SPACING + buffer); // max height -.5
    float minHeight = transform.position.z + (verticalRange.y * GridConstants.Z_SPACING + buffer); // min height 1
    return c.transform.position.z <= minHeight && c.transform.position.z >= maxHeight;
  }
  public float CalculateDamageAfterResistances(Character c)
  {
    if (c != null)
    {
      Debug.Log("damage amount " + damageAmount + " adjusted to " + damageAmount * c.GetDamageMultiplier(damageType));
      return damageAmount * c.GetDamageMultiplier(damageType);
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

  public bool applySlowdown
  {
    get
    {
      return owningWeapon.attachToOwner;
    }
  }

}