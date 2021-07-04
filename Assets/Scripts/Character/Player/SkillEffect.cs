using UnityEngine;
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

public enum SkillEffectMovementProperty
{
  MoveForward,
  MoveUp,
}

[System.Serializable]
public class SkillEffectSet
{
  [Tooltip("Defines whether this effect set should always be executed every time the skill is used")]
  public bool alwaysExecute = true;
  public bool canUseInMidair = false;

  public SkillEffect[] skillEffects;

}

[System.Serializable]
public class SkillEffect
{

  public SkillEffectType useType;
  [Tooltip("Defines whether any skill should be able to interrupt (cancel) this effect and subsequent effects")]
  public bool interruptable = false;

  [Tooltip("Defines whether an input of this skill should end this effect and move to the next")]
  public bool advanceable = false;
  [Tooltip("This effect is bypassed if the skillEffect is queued when this effect is reached. Good for eg pauses between attacks")]
  public bool skipIfQueued = false;
  public float duration;

  public SkillEffectPropertyToFloat properties;
  public SkillEffectMovementPropertyToCurve movement;
  public CharacterVitalToCurveDictionary vitalChanges;
  public List<CharacterMovementAbility> movementAbilities;
  public AttackSpawn[] weaponSpawns;
  public SkillEffect()
  {

  }

  public virtual void BeginSkillEffect(Character owner)
  {
    List<Weapon> weaponInstances = new List<Weapon>();
    foreach (AttackSpawn weaponSpawn in weaponSpawns)
    {
      SpawnWeapon(weaponSpawn, owner, weaponInstances);
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
    return;
  }

  public void SpawnWeapon(AttackSpawn weaponSpawn, Character owner, List<Weapon> weaponInstances)
  {
    Quaternion rotationAngle = Quaternion.AngleAxis(owner.weaponPivotRoot.eulerAngles.z + weaponSpawn.rotationOffset, Vector3.forward);
    Weapon weaponInstance = GameObject.Instantiate(
      weaponSpawn.weaponObject,
      owner.weaponPivotRoot.position + (rotationAngle * new Vector3(weaponSpawn.range, 0, 0)),
      rotationAngle
    );
    weaponInstance.transform.parent = owner.weaponPivotRoot;
    weaponInstance.Init(weaponSpawn, this, owner, weaponInstances);
  }

  public virtual float GetEffectiveRange(Character owner)
  {
    List<float> weaponRanges = new List<float>();
    foreach (AttackSpawn attackSpawn in weaponSpawns)
    {
      weaponRanges.Add(attackSpawn.range + attackSpawn.weaponSize + attackSpawn.attackData.GetCumulativeEffectiveWeaponRange(owner));
    }
    return Mathf.Max(weaponRanges.ToArray());
  }

  public virtual List<SkillRangeInfo> CalculateRangeInfos(Character owner)
  {
    List<SkillRangeInfo> infos = new List<SkillRangeInfo>();
    for (int i = 0; i < weaponSpawns.Length; i++)
    {
      SkillRangeInfo info = new SkillRangeInfo(weaponSpawns[i]);
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
