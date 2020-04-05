using UnityEngine;

public interface IDamageSource
{
  string sourceString { get; }
  bool IsOwnedBy(Character c);
  bool IsSameOwnerType(Character c);
  Vector3 GetKnockbackForCharacter(Character c);
  float invulnerabilityWindow { get; }
  int damageAmount { get; }
  DamageType damageType { get; }
  float stunMagnitude { get; }
  bool ignoresInvulnerability { get; }

  bool forcesItemDrop { get; }

}