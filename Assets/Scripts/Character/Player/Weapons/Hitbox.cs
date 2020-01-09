
using System;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
  public CharacterAttack attack;
  public Character character;
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

  public void Init(CharacterAttack atk, Character ch)
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
  public int GetDamageAmount()
  {
    return attack.GetDamageAmount(character);
  }

  public DamageType GetDamageType()
  {
    return attack.GetDamageType(character);
  }

  public float GetStun()
  {
    return attack.GetStun(character);
  }
  public bool IgnoresInvulnerability()
  {
    return attack.IgnoresInvulnerability();
  }

  public float GetInvulnerabilityWindow()
  {
    return attack.GetInvulnerabilityWindow(character);
  }

  public Vector3 GetKnockback()
  {
    return attack.GetKnockback(character, this);
  }
}