using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WeaponAction
{
  public WeaponActionType type;
  public NormalizedCurve motion;
}

public enum WeaponActionType
{
  Move = 0,
  MoveVertical = 7,
  Wait = 1,
  RotateRelative = 2,
  RotateRelativeVertical = 6,
  Homing = 3,
  HomingVertical = 8,
  Scale = 4,
  MarkDone = 5,
}

[System.Serializable]
public class HomingParams
{
  [Tooltip("max distance at which weapon will home, horizontally")]
  public float homingRange = 0f;

  [Tooltip("greatest angle between weapon rotation (on z axis) and target")]
  public float maxAngleToTarget = 0f;

  [Tooltip("max distance at which weapon will home, vertically")]
  public float verticalHomingRange = 0f;
}
[System.Serializable]
public class Attack
{
  public bool destroyOnContact;
  public AttackSpawn objectToSpawn;
  public bool spawnObjectOnContact;
  public bool spawnObjectOnDestruction;
  public Overrideable<float> duration;
  public HomingParams homing;
  public WeaponAction[] weaponActions;

  public float GetCumulativeEffectiveWeaponRange(Character owner)
  {
    float ownRange = 0;
    float cur = ownRange;
    // foreach (WeaponActionGroup actionGroup in weaponActionGroups)
    // {
    foreach (WeaponAction action in weaponActions)
    {
      if (action.type == WeaponActionType.Move)
      {
        cur += action.motion.magnitude.Resolve(owner);
        ownRange = Mathf.Max(cur, ownRange);
      }
    }
    // }
    float childRange = 0;
    if (objectToSpawn != null && objectToSpawn.attackData != null && spawnObjectOnDestruction)
    {
      childRange = objectToSpawn.weaponSize + objectToSpawn.attackData.GetCumulativeEffectiveWeaponRange(owner);
    }
    return ownRange + childRange;
  }

  public SkillRangeInfo GetAttackRangeInfo(ref SkillRangeInfo info, Character owner, float initialWeaponSize, float initialRange, float initialAngle)
  {
    float currentRange = initialWeaponSize + initialRange;
    float currentAngle = initialAngle;
    foreach (WeaponAction action in weaponActions)
    {
      if (action.type == WeaponActionType.Move)
      {
        currentRange += action.motion.magnitude.Resolve(owner);
        info.minRange = Mathf.Min(currentRange, info.minRange);
        info.maxRange = Mathf.Max(currentRange, info.maxRange);
      }
      else if (action.type == WeaponActionType.RotateRelative)
      {
        currentAngle += action.motion.magnitude.Resolve(owner);
        info.minAngle = Mathf.Min(currentAngle, info.minAngle);
        info.maxAngle = Mathf.Max(currentAngle, info.maxAngle);
      }
      else if (action.type == WeaponActionType.Scale)
      {
        if (action.motion.magnitude.Resolve(owner) > 1)
        {
          currentRange -= initialWeaponSize; // default weapon size already included, so need to deduct it if we also scale
          currentRange += initialWeaponSize * action.motion.magnitude.Resolve(owner);
          info.minRange = Mathf.Min(currentRange, info.minRange);
          info.maxRange = Mathf.Max(currentRange, info.maxRange);
        }
      }
    }
    if (objectToSpawn != null && objectToSpawn.attackData != null && spawnObjectOnDestruction)
    {
      objectToSpawn.attackData.GetAttackRangeInfo(ref info, owner, objectToSpawn.weaponSize, currentRange, currentAngle);
    }
    return info;
  }
}
