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
  Move,
  Wait,
  RotateRelative,
  MarkDone
}

public class HomingParams
{
  public float homingRange = 0f;
  public float maxAngleToTarget = 0f;
}
[System.Serializable]
public class Attack
{
  public bool destroyOnContact;
  public AttackSpawn objectToSpawn;
  public bool spawnObjectOnContact;
  public bool spawnObjectOnDestruction;
  public Overrideable<float> duration;
  public WeaponAction[] weaponActions;
  public HomingParams homing;

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

  public SkillRangeInfo GetAttackRangeInfo(ref SkillRangeInfo info, Character owner, float initialRange, float initialAngle)
  {
    float currentRange = initialRange;
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
    }
    if (objectToSpawn != null && objectToSpawn.attackData != null && spawnObjectOnDestruction)
    {
      objectToSpawn.attackData.GetAttackRangeInfo(ref info, owner, objectToSpawn.weaponSize + currentRange, currentAngle);
    }
    return info;
  }
}
