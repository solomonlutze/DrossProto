
using System;
using UnityEngine;

public class EnvironmentalDamage : DamageSource
{
    public EnvironmentTile tileType;
    public Character character;

    public override string sourceString
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

    public override int GetDamageAmount()
    {
        return tileType.environmentalDamageInfo.damageAmount;
    }

    public override DamageType GetDamageType()
    {
        return tileType.environmentalDamageInfo.damageType;
    }

    public override float GetStun()
    {
        return tileType.environmentalDamageInfo.stun;
    }

    public override bool IgnoresInvulnerability()
    {
        return tileType.environmentalDamageInfo.ignoreInvulnerability;
    }

    public override float GetInvulnerabilityWindow(Character c)
    {
        return tileType.environmentalDamageInfo.invulnerabilityWindow;
    }

    public int GetResistanceRequiredForImmunity()
    {
        return tileType.environmentalDamageInfo.resistanceRequiredForImmunity;
    }
}