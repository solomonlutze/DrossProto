using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;

public enum SkillEffectType { OneTime, Continuous }

[System.Serializable]
public class SkillEffectGroup
{
  SkillEffect[] skillEffects;
}

public enum SkillEffectFloatProperty
{
  MoveSpeed,

  RotationSpeed
}

public enum SkillEffectDamageMultiplierProperty
{
  AcidDamageMultiplier = DamageType.Acid,
  FungalDamageMultiplier = DamageType.Fungal,
  HeatDamageMultiplier = DamageType.Heat,
  PhysicalDamageMultiplier = DamageType.Physical
}

public enum SkillEffectMovementProperty
{
  MoveForward,
  MoveUp,
}

[System.Serializable]
public class SkillEffectSet
{

  public string name;
  [Tooltip("Defines whether this effect set should always be executed every time the skill is used")]
  public bool alwaysExecute = true;
  public bool canUseInMidair = false;

  public SkillEffect[] skillEffects;
  public float GetTotalStaminaCost(Character c)
  {
    float cost = 0;
    foreach (SkillEffect effect in skillEffects)
    {
      cost += effect.staminaCost.Resolve(c);
    }
    return cost;
  }

}

[System.Serializable]
public class SkillEffect
{

  public string name;
  public Overrideable<bool> shouldExecute = new Overrideable<bool>(true);
  public Overrideable<float> staminaCost;
  public SkillEffectType useType;
  [Tooltip("Defines whether taking damage should interrupt/end this effect and subsequent effects")]
  public Overrideable<bool> interruptable = new Overrideable<bool>(false);
  [Tooltip("Defines whether this (or another) skill can be used to interrupt this effect and subsequent effects")]
  public Overrideable<bool> cancelable = new Overrideable<bool>(false);
  [Tooltip("If this effect is canceled, the character should instantly change directions. Mostly for scramble skills.")]
  public Overrideable<bool> reverseDirectionIfCanceled = new Overrideable<bool>(false);
  [Tooltip("Indicates a scramble action. Used for determining whether other skills can be used.")]
  public Overrideable<bool> scrambling = new Overrideable<bool>(false);

  [Tooltip("Defines whether an input of this skill should end this effect and move to the next")]
  public Overrideable<bool> advanceable = new Overrideable<bool>(false);
  [Tooltip("Defines whether the effect should end when the character touches the ground")]
  public Overrideable<bool> endOnGrounded = new Overrideable<bool>(false);
  [Tooltip("Defines whether the effect should be skipped if the character is not grounded")]
  public Overrideable<bool> skipIfAirborne = new Overrideable<bool>(false);
  [Tooltip("Defines whether the effect should end when the character is no longer inputting skill (ie button release). IGNORED FOR ONE TIME SKILLS.")]
  public Overrideable<bool> endOnInputRelease = new Overrideable<bool>(true);
  [Tooltip("This effect is bypassed if the skillEffect is queued when this effect is reached. Good for eg pauses between attacks")]
  public Overrideable<bool> skipIfQueued = new Overrideable<bool>(false);
  [Tooltip("This effect is repeated if the skillEffect is queued when the end of the effect is reached.")]
  public Overrideable<bool> isRepeatable = new Overrideable<bool>(false);
  [Tooltip("Animation only. Sets the 'IsGuarding' flag on the animator.")]
  public bool isGuarding;
  [Tooltip("This skill resets the character's visuals.")]
  public bool restoreBrokenParts;

  [FormerlySerializedAs("duration")]
  [Tooltip("Min time to spend in skill effect. Always define this!")]
  public Overrideable<float> minDuration;

  [Tooltip("Max time to spend in skill effect. Only for continuous effects!")]
  public Overrideable<float> maxDuration;

  public SkillEffectPropertyToFloat properties;
  public SkillEffectDamageMultiplierToFloat damageMultipliers;
  public SkillEffectMovementPropertyToCurve movement;
  public BodyPartVitalToCurveDictionary partVitalChanges;
  public DamageTypeToCurveDictionary buildupChanges;
  public List<CharacterMovementAbility> movementAbilities;

  [Tooltip("Charge level increases by 1 if we've spent more time in the effect than the charge level requires")]
  public Overrideable<float>[] chargeLevels;
  public AttackSpawn[] weaponSpawns;
  public SkillEffect()
  {

  }

  public virtual void BeginSkillEffect(Character owner)
  {
    if (restoreBrokenParts)
    {
      owner.RestoreBrokenParts();
    }
    foreach (AttackSpawn weaponSpawn in weaponSpawns)
    {
      SpawnWeapon(weaponSpawn, owner);
    }
    if (chargeLevels.Length > 0)
    {
      owner.chargingUpParticleSystem.Play();
    }
  }
  public virtual void DoSkillEffect(Character owner)
  {
    foreach (KeyValuePair<BodyPartVital, NormalizedCurve> vitalChange in partVitalChanges)
    {
      foreach (PartStatusInfo part in owner.partStatusInfos.Values)
      {
        switch (vitalChange.Key)
        { // this operation could probably live on PartStatusInfo. Don't overthink it for now
          case BodyPartVital.CurrentDamage:
            part.AdjustCurrentDamage(owner.CalculateCurveProgressIncrement(vitalChange.Value, false, useType == SkillEffectType.Continuous), isNonbreaking: true, exhausts: false);
            break;
          case BodyPartVital.CurrentExertion:
            part.AdjustCurrentExertion(owner.CalculateCurveProgressIncrement(vitalChange.Value, false, useType == SkillEffectType.Continuous), isBreaking: false);
            break;
          case BodyPartVital.CurrentBreakingPoint_Percent:
            part.AdjustBreakingPoint_Percent(owner.CalculateCurveProgressIncrement(vitalChange.Value, false, useType == SkillEffectType.Continuous));
            break;
            // TODO: adjust breaking point by molting cost (per-part)
        }
      }

    }
    foreach (KeyValuePair<DamageType, NormalizedCurve> buildupChange in buildupChanges)
    {
      owner.AdjustElementalDamageBuildup(buildupChange.Key, owner.CalculateCurveProgressIncrement(buildupChange.Value, false, useType == SkillEffectType.Continuous));
    }
    if (weaponSpawns.Length > 0 && owner.currentAttackSpawnIndex < weaponSpawns.Length)
    {
      if (weaponSpawns[owner.currentAttackSpawnIndex].afterPrevious)
      {
        Debug.LogError("WARNING: afterPrevious for attack spawn not implemented!");
      }
      if (owner.timeSpentInSkillEffect > weaponSpawns[owner.currentAttackSpawnIndex].delay.Resolve(owner))
      {
        SpawnWeapon(weaponSpawns[owner.currentAttackSpawnIndex], owner);
        owner.currentAttackSpawnIndex++;
      }
    }
    if (chargeLevels.Length > 0 && owner.chargeLevel < chargeLevels.Length)
    {
      if (owner.timeSpentInSkillEffect > chargeLevels[owner.chargeLevel].Resolve(owner))
      {
        owner.chargeLevel++;
        owner.chargeLevelIncreaseParticleSystem.Play();
        if (owner.chargeLevel == chargeLevels.Length)
        {
          owner.chargingUpParticleSystem.Stop();
          owner.fullyChargedParticleSystem.Play();
        }
      }
    }
    return;
  }

  public virtual void EndSkillEffect(Character owner)
  {
    owner.animator.SetBool("IsGuarding", false);
    owner.chargingUpParticleSystem.Stop();
    owner.fullyChargedParticleSystem.Stop();
  }
  public void SpawnWeapon(AttackSpawn weaponSpawn, Character owner, Transform spawnTransformOverride = null)
  {
    Transform spawnTransform = spawnTransformOverride ? spawnTransformOverride : owner.weaponPivotRoot;
    Quaternion rotationAngleHorizontal = Quaternion.AngleAxis(spawnTransform.eulerAngles.z + weaponSpawn.rotationOffset.get(owner), Vector3.forward);
    Quaternion rotationAngleVertical = Quaternion.AngleAxis(spawnTransform.eulerAngles.y + weaponSpawn.verticalRotationOffset.get(owner), Vector3.up);
    Quaternion rotationAngle = rotationAngleHorizontal * rotationAngleVertical;
    Weapon weaponInstance = GameObject.Instantiate(
      weaponSpawn.weaponObject,
      spawnTransform.position + (rotationAngle * new Vector3(weaponSpawn.range.get(owner), 0, WorldObject.ConvertNormalizedZDistanceToWorldspace(weaponSpawn.verticalSpawnDistance.get(owner)))),
      rotationAngle
    );
    weaponInstance.transform.parent = owner.weaponPivotRoot;
    weaponInstance.Init(weaponSpawn, this, owner);
    if (!weaponSpawn.attachToOwner)
    {
      weaponInstance.transform.parent = null; // we want to instantiate relative to the weaponPivot and then immediately leave the hierarchy
    }
  }

  // public virtual float GetEffectiveRange(Character owner)
  // {
  //   List<float> weaponRanges = new List<float>();
  //   foreach (AttackSpawn attackSpawn in weaponSpawns)
  //   {
  //     weaponRanges.Add(attackSpawn.range.get(owner) + attackSpawn.weaponSize + attackSpawn.attackData.GetCumulativeEffectiveWeaponRange(owner));
  //   }
  //   return Mathf.Max(weaponRanges.ToArray());
  // }

  public virtual List<SkillRangeInfo> CalculateRangeInfos(Character owner)
  {
    List<SkillRangeInfo> infos = new List<SkillRangeInfo>();
    for (int i = 0; i < weaponSpawns.Length; i++)
    {
      SkillRangeInfo info = new SkillRangeInfo(weaponSpawns[i], owner);
      infos.Add(weaponSpawns[i].attackData.GetAttackRangeInfo(ref info, owner, weaponSpawns[i].weaponSize, info.minRange, info.maxAngle));
    }
    return infos;
  }

  public virtual float CalculateDefaultStaminaCost()
  {
    return 0;
  }

  float GetDurationMultiplier(Character owner)
  {
    return owner.IsActiveSkillPartBroken() ? DrossConstants.BROKEN_PART_EFFECT_DURATION_MULTIPLIER : 1f;
  }
  public float GetMinDuration(Character owner)
  {
    return minDuration.get(owner) * GetDurationMultiplier(owner);
  }
  public float GetMaxDuration(Character owner)
  {
    return maxDuration.get(owner) * GetDurationMultiplier(owner);
  }
  int _previousAttackSpawnCount = 0;

}
