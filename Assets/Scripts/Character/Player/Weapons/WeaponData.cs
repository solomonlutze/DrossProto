using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class Moveset
{
    public AttackTypeToSkillEffectDictionary attacks;

    public Moveset()
    {
        attacks = new AttackTypeToSkillEffectDictionary();
    }

    public Moveset(TraitSlotToTraitDictionary traits)
    {
        attacks = new AttackTypeToSkillEffectDictionary();
        foreach (TraitSlot slot in traits.Keys)
        {
            attacks[Character.GetAttackTypeForTraitSlot(slot)] = traits[slot].moveset.attacks[Character.GetAttackTypeForTraitSlot(slot)];
        }
    }
}

public class WeaponData : ScriptableObject
{
    public Weapon weaponObject;
    public AttackTypeToSkillEffectDictionary attacks;

    public WeaponData()
    {
        attacks = new AttackTypeToSkillEffectDictionary();
    }

#if UNITY_EDITOR
    [MenuItem("Assets/Create/Weapon")]
    public static void CreateWeaponData()
    {
        string path = EditorUtility.SaveFilePanelInProject("SaveWeapon", "NewWeapon", "Asset", "SaveWeapon", "Assets/resources/Data/CharacterData/WeaponData");
        if (path == "")
            return;
        WeaponData weaponInstance = ScriptableObject.CreateInstance<WeaponData>();
        AttackSkillEffect basicSkillInstance = ScriptableObject.CreateInstance<AttackSkillEffect>();
        weaponInstance.attacks[AttackType.Basic] = basicSkillInstance;

        AttackSkillEffect dashSkillInstance = ScriptableObject.CreateInstance<AttackSkillEffect>();
        weaponInstance.attacks[AttackType.Dash] = dashSkillInstance;

        AttackSkillEffect blockingSkillInstance = ScriptableObject.CreateInstance<AttackSkillEffect>();
        weaponInstance.attacks[AttackType.Blocking] = blockingSkillInstance;

        AttackSkillEffect chargeSkillInstance = ScriptableObject.CreateInstance<AttackSkillEffect>();
        weaponInstance.attacks[AttackType.Charge] = chargeSkillInstance;

        AttackSkillEffect criticalSkillInstance = ScriptableObject.CreateInstance<AttackSkillEffect>();
        weaponInstance.attacks[AttackType.Critical] = criticalSkillInstance;

        AssetDatabase.CreateAsset(weaponInstance, path);
        int index = path.IndexOf(".");
        if (index > 0)
            path = path.Substring(0, index);
        AssetDatabase.CreateAsset(basicSkillInstance, path + "BasicSkillEffect.Asset");
        AssetDatabase.CreateAsset(dashSkillInstance, path + "DashSkillEffect.Asset");
        AssetDatabase.CreateAsset(blockingSkillInstance, path + "BlockingSkillEffect.Asset");
        AssetDatabase.CreateAsset(chargeSkillInstance, path + "ChargeSkillEffect.Asset");
        AssetDatabase.CreateAsset(criticalSkillInstance, path + "CriticalSkillEffect.Asset");
    }
#endif
}