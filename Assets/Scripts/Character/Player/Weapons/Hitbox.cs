
using System;
using UnityEngine;

public class Hitbox : DamageSource
{
    public CharacterAttack attack;
    public Character character;

    public void Init(CharacterAttack atk, Character ch)
    {
        attack = atk;
        character = ch;
    }

    public override bool IsOwnedBy(Character c)
    {
        return c == character;
    }

    public override bool IsSameOwnerType(Character c)
    {
        return character && character.characterType == c.characterType;
    }
    public override int GetDamageAmount()
    {
        return attack.GetDamageAmount(character);
    }

    public override DamageType GetDamageType()
    {
        return attack.GetDamageType(character);
    }

    public override float GetStun()
    {
        return attack.GetStun(character);
    }
    public override bool IgnoresInvulnerability()
    {
        return attack.IgnoresInvulnerability();
    }

    public override float GetInvulnerabilityWindow(Character c)
    {
        return attack.GetInvulnerabilityWindow(character);
    }

    public override Vector3 GetKnockback()
    {
        return attack.GetKnockback(character, this);
    }

    public override bool ForcesItemDrop()
    {
        return attack.ForcesItemDrop();
    }
}