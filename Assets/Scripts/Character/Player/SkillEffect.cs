using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;
// NOTE: WhileAirborne means the effect lasts until the player lands no matter what.
// If something else should ALSO end the effect, make a new flag
public enum SkillEffectType { OneTime, Continuous, WhileAirborne }

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

  [Tooltip("Defines whether an input of this skill should end this effect and move to the next")]
  public Overrideable<bool> advanceable = new Overrideable<bool>(false);
  [Tooltip("This effect is bypassed if the skillEffect is queued when this effect is reached. Good for eg pauses between attacks")]
  public Overrideable<bool> skipIfQueued = new Overrideable<bool>(false);
  [Tooltip("Animation only. Sets the 'IsGuarding' flag on the animator.")]
  public bool isGuarding;
  [Tooltip("This skill resets the character's visuals.")]
  public bool restoreCharacterVisuals;

  [FormerlySerializedAs("duration")]
  [Tooltip("Min time to spend in skill effect. Always define this!")]
  public Overrideable<float> minDuration;

  [Tooltip("Max time to spend in skill effect. Only for continuous effects!")]
  public Overrideable<float> maxDuration;

  public SkillEffectPropertyToFloat properties;
  public SkillEffectDamageMultiplierToFloat damageMultipliers;
  public SkillEffectMovementPropertyToCurve movement;
  public CharacterVitalToCurveDictionary vitalChanges;
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
    if (restoreCharacterVisuals)
    {
      owner.InitializeVisuals();
    }
    List<Weapon> weaponInstances = new List<Weapon>();
    foreach (AttackSpawn weaponSpawn in weaponSpawns)
    {
      SpawnWeapon(weaponSpawn, owner, weaponInstances);
    }
    if (chargeLevels.Length > 0)
    {
      owner.chargingUpParticleSystem.Play();
    }
  }
  public virtual void DoSkillEffect(Character owner)
  {
    foreach (KeyValuePair<CharacterVital, NormalizedCurve> vitalChange in vitalChanges)
    {
      switch (vitalChange.Key)
      {
        case CharacterVital.CurrentHealth:
          owner.AdjustCurrentHealth(owner.CalculateCurveProgressIncrement(vitalChange.Value, false, useType == SkillEffectType.Continuous));
          break;
        case CharacterVital.CurrentMaxHealth:
          owner.AdjustCurrentMaxHealth(owner.CalculateCurveProgressIncrement(vitalChange.Value, false, useType == SkillEffectType.Continuous));
          break;
      }
    }
    foreach (KeyValuePair<DamageType, NormalizedCurve> buildupChange in buildupChanges)
    {
      owner.AdjustElementalDamageBuildup(buildupChange.Key, owner.CalculateCurveProgressIncrement(buildupChange.Value, false, useType == SkillEffectType.Continuous));
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
  public void SpawnWeapon(AttackSpawn weaponSpawn, Character owner, List<Weapon> weaponInstances, Transform spawnTransformOverride = null)
  {
    Transform spawnTransform = spawnTransformOverride ? spawnTransformOverride : owner.weaponPivotRoot;
    Quaternion rotationAngle = Quaternion.AngleAxis(spawnTransform.eulerAngles.z + weaponSpawn.rotationOffset.get(owner), Vector3.forward);
    Weapon weaponInstance = GameObject.Instantiate(
      weaponSpawn.weaponObject,
      spawnTransform.position + (rotationAngle * new Vector3(weaponSpawn.range.get(owner), 0, 0)),
      rotationAngle
    );
    weaponInstance.transform.parent = owner.weaponPivotRoot;
    weaponInstance.Init(weaponSpawn, this, owner, weaponInstances);
    if (!weaponSpawn.attachToOwner)
    {
      weaponInstance.transform.parent = null; // we want to instantiate relative to the weaponPivot and then immediately leave the hierarchy
    }
  }

  public virtual float GetEffectiveRange(Character owner)
  {
    List<float> weaponRanges = new List<float>();
    foreach (AttackSpawn attackSpawn in weaponSpawns)
    {
      weaponRanges.Add(attackSpawn.range.get(owner) + attackSpawn.weaponSize + attackSpawn.attackData.GetCumulativeEffectiveWeaponRange(owner));
    }
    return Mathf.Max(weaponRanges.ToArray());
  }

  public virtual List<SkillRangeInfo> CalculateRangeInfos(Character owner)
  {
    List<SkillRangeInfo> infos = new List<SkillRangeInfo>();
    for (int i = 0; i < weaponSpawns.Length; i++)
    {
      SkillRangeInfo info = new SkillRangeInfo(weaponSpawns[i], owner);
      infos.Add(weaponSpawns[i].attackData.GetAttackRangeInfo(ref info, owner, info.maxRange, info.maxAngle));
    }
    return infos;
  }

  public virtual float CalculateDefaultStaminaCost()
  {
    return 0;
  }

  int _previousAttackSpawnCount = 0;

}
