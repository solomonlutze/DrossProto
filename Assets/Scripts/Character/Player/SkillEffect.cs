using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SkillEffectType { OneTime, Continuous }

[System.Serializable]
public class SkillEffectGroup
{
  SkillEffect[] skillEffects;
}

public enum SkillEffectProperty
{
  Move,
  MoveSpeed,

  RotationSpeed
}

[System.Serializable]
public class SkillEffect
{

  public SkillEffectType useType;

  public bool interruptable = false;
  public bool alwaysExecute = true;
  public float duration;

  public SkillEffectPropertyToFloat properties;
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

  public virtual float GetEffectiveRange()
  {
    List<float> weaponRanges = new List<float>();
    foreach (AttackSpawn attackSpawn in weaponSpawns)
    {
      weaponRanges.Add(attackSpawn.range + attackSpawn.weaponSize + attackSpawn.attackData.GetCumulativeEffectiveWeaponRange());
    }
    return Mathf.Max(weaponRanges.ToArray());
  }

  public virtual List<SkillRangeInfo> CalculateRangeInfos()
  {
    List<SkillRangeInfo> infos = new List<SkillRangeInfo>();
    for (int i = 0; i < weaponSpawns.Length; i++)
    {
      SkillRangeInfo info = new SkillRangeInfo(weaponSpawns[i]);
      // infos.Add(weaponSpawns[i].attackData.GetAttackRangeInfo(ref info, info.maxRange, info.maxAngle));
    }
    return infos;
  }

  public virtual float CalculateDefaultStaminaCost()
  {
    return 0;
  }

  int _previousAttackSpawnCount = 0;

}
