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

  public WeaponVariable owningWeaponDataWeapon;

  [Tooltip("If not populated, weaponObject is object of parent Weapon data")]
  public Weapon weaponObjectOverride;

  [HideInInspector]
  public Weapon weaponObject
  {
    get
    {
      if (weaponObjectOverride != null)
      {
        return weaponObjectOverride;
      }
      return owningWeaponDataWeapon.Value;
    }
  }
  public float range;

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
  public float rotationOffset;
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

  public DamageInfo baseDamage;

  public AttackSkillEffect(WeaponVariable weapon)
  {
    weaponSpawns = new AttackSpawn[] {
        new AttackSpawn(weapon)
      };
  }

  public override void DoSkillEffect(Character owner)
  {
    // Transform weaponParent = new 
    // owner.weaponPivot.eulerAngles = new Vector3(0, 0, rotationOffset); // shrug?
    List<Weapon> weaponInstances = new List<Weapon>();
    foreach (AttackSpawn weaponSpawn in weaponSpawns)
    {
      SpawnWeapon(weaponSpawn, owner, weaponInstances);
    }
  }

  public void SpawnWeapon(AttackSpawn weaponSpawn, Character owner, List<Weapon> weaponInstances)
  {
    Quaternion rotationAngle = Quaternion.AngleAxis(owner.weaponPivotRoot.eulerAngles.z + weaponSpawn.rotationOffset, Vector3.forward);
    Weapon weaponInstance = GameObject.Instantiate(
      weaponSpawn.weaponObject,
      owner.weaponPivotRoot.position + (rotationAngle * new Vector3(weaponSpawn.range, 0, 0)),
      // Quaternion.AngleAxis(owner.weaponPivotRoot.eulerAngles.z + weaponSpawn.rotationOffset, Vector3.forward)
      rotationAngle
    );
    weaponInstance.Init(weaponSpawn.attackData, this, owner, weaponInstances); // weaponInstance.transform.parent = null; // we want to instantiate relative to the weaponPivot and then immediately leave the hierarchy
    // owner.StartCoroutine(weaponInstance.PerformWeaponActions());
  }

  public override float GetEffectiveRange()
  {
    List<float> weaponRanges = new List<float>();
    foreach (AttackSpawn attackSpawn in weaponSpawns)
    {
      weaponRanges.Add(attackSpawn.range + attackSpawn.weaponSize + attackSpawn.attackData.GetCumulativeEffectiveWeaponRange());
    }
    return Mathf.Max(weaponRanges.ToArray());
  }

  public override List<SkillRangeInfo> CalculateRangeInfos()
  {
    List<SkillRangeInfo> infos = new List<SkillRangeInfo>();
    for (int i = 0; i < weaponSpawns.Length; i++)
    {
      SkillRangeInfo info = new SkillRangeInfo(weaponSpawns[i]);
      infos.Add(weaponSpawns[i].attackData.GetAttackRangeInfo(ref info, info.maxRange, info.maxAngle));
    }
    return infos;
  }

  // #if UNITY_EDITOR
  //     [MenuItem("Assets/Create/Skills/AttackSkillEffect")]
  //     public static void CreateAttackSkillEffect()
  //     {
  //         string path = EditorUtility.SaveFilePanelInProject("Save Attack Skill Effect", "New Attack Skill Effect", "Asset", "Save Attack Skill Effect", "Assets/resources/Data/CharacterData/Skills/AttackSkillEffects");
  //         if (path == "")
  //             return;
  //         AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AttackSkillEffect>(), path);
  //     }
  // #endif
}

