using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using ScriptableObjectArchitecture;

public enum AttackType
{
  Basic,
  Dash,
  Blocking,

  Charge,
  Critical
}

[System.Serializable]
public class AttackSpawn
{
  public Attack attackData;
  public DamageInfo damage;

  public WeaponVariable owningWeaponDataWeapon;

  public Weapon weaponObject
  {
    get
    {
      return owningWeaponDataWeapon?.Value;
    }
  }
  public Overrideable<float> range;
  public bool afterPrevious;
  public Overrideable<float> delay;
  public Overrideable<float> verticalSpawnDistance;

  //TODO: Account for multiple hitboxes and non-box collider hitboxes!!
  public float weaponSize
  {
    get
    {
      if (weaponObject?.weaponBody != null)
      {
        return weaponObject.weaponBody.localScale.x;
      }
      return 0f;
    }
  }
  public Overrideable<float> rotationOffset;
  public Overrideable<float> verticalRotationOffset;
  public bool attachToOwner = true;
  public AttackSpawn()
  {

  }
  public AttackSpawn(WeaponVariable weapon)
  {
    owningWeaponDataWeapon = weapon;
  }
}
// A single skill effect that spawns a single weapon
[System.Serializable]
public class AttackSkillEffect : SkillEffect
{
  public override void DoSkillEffect(Character owner)
  {
    Debug.LogError("somebody trying to use attackSkillEffect! stop them!");
    // Transform weaponParent = new 
    // owner.weaponPivot.eulerAngles = new Vector3(0, 0, rotationOffset); // shrug?
  }

}

