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
public class AttackSpawn
{
    public float delay;
    public Attack attackData;
    public Weapon weaponObject;
    public float range;
    public float rotationOffset;
}
// A single skill effect that spawns a single weapon
[System.Serializable]
public class AttackSkillEffect : SkillEffect
{

    public AttackSpawn[] weaponSpawns;
    public DamageInfo baseDamage;

    public override IEnumerator ActivateSkillEffect(Character owner)
    {
        // Transform weaponParent = new 
        // owner.weaponPivot.eulerAngles = new Vector3(0, 0, rotationOffset); // shrug?
        List<Weapon> weaponInstances = new List<Weapon>();
        foreach (AttackSpawn weaponSpawn in weaponSpawns)
        {
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
        Quaternion rotationAngle = Quaternion.AngleAxis(owner.weaponPivotRoot.position.z + weaponSpawn.rotationOffset, Vector3.forward);
        Weapon weaponInstance = Instantiate(
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

