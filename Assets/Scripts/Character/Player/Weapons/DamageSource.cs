using System.Collections.Generic;
using UnityEngine;

public interface IDamageSource
{
    string sourceString { get; }
    bool IsOwnedBy(Character c);
    bool IsSameOwnerType(Character c);
    Vector3 GetKnockbackForCharacter(Character c);
    List<CharacterMovementAbility> movementAbilitiesWhichBypassDamage { get; }
    float CalculateDamageAfterResistances(Character c);
    float invulnerabilityWindow { get; }
    bool isNonlethal { get; }
    int damageAmount { get; }
    DamageType damageType { get; }
    float stunMagnitude { get; }
    bool ignoresInvulnerability { get; }

    bool forcesItemDrop { get; }

}