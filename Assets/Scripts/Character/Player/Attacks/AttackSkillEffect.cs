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
  public float delay;
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

  public AttackSpawn[] weaponSpawns;
  public DamageInfo baseDamage;

  public AttackSkillEffect(WeaponVariable weapon)
  {
    weaponSpawns = new AttackSpawn[] {
        new AttackSpawn(weapon)
      };
  }

  public override IEnumerator ActivateSkillEffect(Character owner)
  {
    // Transform weaponParent = new 
    // owner.weaponPivot.eulerAngles = new Vector3(0, 0, rotationOffset); // shrug?
    List<Weapon> weaponInstances = new List<Weapon>();
    Debug.Log("activating skill effect");
    foreach (AttackSpawn weaponSpawn in weaponSpawns)
    {
      Debug.Log("spawning" + weaponSpawn.weaponObject);
      yield return SpawnWeapon(weaponSpawn, owner, weaponInstances);
    }
    while (weaponInstances.Count > 0)
    {
      yield return null;
    }
  }

  public IEnumerator SpawnWeapon(AttackSpawn weaponSpawn, Character owner, List<Weapon> weaponInstances)
  {
    yield return new WaitForSeconds(weaponSpawn.delay);
    Quaternion rotationAngle = Quaternion.AngleAxis(owner.weaponPivotRoot.eulerAngles.z + weaponSpawn.rotationOffset, Vector3.forward);
    Debug.Log("rotationAngle: " + rotationAngle);
    Weapon weaponInstance = GameObject.Instantiate(
      weaponSpawn.weaponObject,
      owner.weaponPivotRoot.position + (rotationAngle * new Vector3(weaponSpawn.range, 0, 0)),
      // Quaternion.AngleAxis(owner.weaponPivotRoot.eulerAngles.z + weaponSpawn.rotationOffset, Vector3.forward)
      rotationAngle
    );
    weaponInstance.Init(weaponSpawn.attackData, this, owner, weaponInstances); // weaponInstance.transform.parent = null; // we want to instantiate relative to the weaponPivot and then immediately leave the hierarchy
    owner.StartCoroutine(weaponInstance.PerformWeaponActions());
  }

  public override float GetEffectiveRange()
  {
    List<float> weaponRanges = new List<float>();
    foreach (AttackSpawn weapon in weaponSpawns)
    {
      weaponRanges.Add(weapon.range + weapon.weaponObject.GetCumulativeEffectiveWeaponRange());
    }
    return Mathf.Max(weaponRanges.ToArray());
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

