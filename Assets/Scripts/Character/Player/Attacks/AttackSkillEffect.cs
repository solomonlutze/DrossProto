using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public enum AttackType
{
  Basic,
  Dash,
  Blocking,

  Charge,
  Critical
}

[System.Serializable]
public class WeaponSpawn
{
  public float delay;
  public Weapon weaponPrefab;
  public float range;
  public float rotationOffset;
}
// A single skill effect that spawns a single weapon
[System.Serializable]
public class AttackSkillEffect : SkillEffect
{

  public WeaponSpawn[] weaponSpawns;
  public Character owner;
  public DamageInfo baseDamage;
  public List<Weapon> weaponInstances;

  public override IEnumerator ActivateSkillEffect(Character c)
  {
    // Transform weaponParent = new 
    // owner.weaponPivot.eulerAngles = new Vector3(0, 0, rotationOffset); // shrug?
    owner = c;
    List<Weapon> weaponInstances = new List<Weapon>();
    foreach (WeaponSpawn weaponSpawn in weaponSpawns)
    {
      yield return SpawnWeapon(weaponSpawn, owner.weaponPivotRoot.position, owner.weaponPivotRoot.eulerAngles);
    }
    while (weaponInstances.Count > 0)
    {
      yield return null;
    }
  }

  public IEnumerator SpawnWeapon(WeaponSpawn weaponSpawn, Vector3 originPosition, Vector3 originEulerAngles)
  {
    yield return new WaitForSeconds(weaponSpawn.delay);
    Quaternion rotationAngle = Quaternion.AngleAxis(originEulerAngles.z + weaponSpawn.rotationOffset, Vector3.forward);
    Weapon weaponInstance = Instantiate(
      weaponSpawn.weaponPrefab,
      originPosition + (rotationAngle * new Vector3(weaponSpawn.range, 0, 0)),
      // Quaternion.AngleAxis(owner.weaponPivotRoot.eulerAngles.z + weaponSpawn.rotationOffset, Vector3.forward)
      rotationAngle
    );
    weaponInstance.Init(this, weaponInstances); // weaponInstance.transform.parent = null; // we want to instantiate relative to the weaponPivot and then immediately leave the hierarchy
    owner.StartCoroutine(weaponInstance.PerformWeaponActions());
  }

  public override float GetEffectiveRange()
  {
    List<float> weaponRanges = new List<float>();
    foreach (WeaponSpawn weapon in weaponSpawns)
    {
      weaponRanges.Add(weapon.range + weapon.weaponPrefab.GetCumulativeEffectiveWeaponRange());
    }
    return Mathf.Max(weaponRanges.ToArray());
  }

#if UNITY_EDITOR
  [MenuItem("Assets/Create/Skills/AttackSkillEffect")]
  public static void CreateAttackSkillEffect()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Attack Skill Effect", "New Attack Skill Effect", "Asset", "Save Attack Skill Effect", "Assets/resources/Data/CharacterData/Skills/AttackSkillEffects");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AttackSkillEffect>(), path);
  }
#endif
}

