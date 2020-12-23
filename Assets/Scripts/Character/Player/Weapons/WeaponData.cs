using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class Moveset
{
    public AttackTypeToCharacterSkillDataDictionary attacks;

    public Moveset()
    {
        attacks = new AttackTypeToCharacterSkillDataDictionary();
    }

    public Moveset(TraitSlotToTraitDictionary traits)
    {
        attacks = new AttackTypeToCharacterSkillDataDictionary();
        foreach (TraitSlot slot in traits.Keys)
        {
            attacks[Character.GetAttackTypeForTraitSlot(slot)] = traits[slot].weaponData.attacks[Character.GetAttackTypeForTraitSlot(slot)];
        }
    }
}

public class WeaponData : ScriptableObject
{
    public Weapon weaponObject;
    public AttackTypeToCharacterSkillDataDictionary attacks;

    public WeaponData()
    {
        attacks = new AttackTypeToCharacterSkillDataDictionary();
    }

#if UNITY_EDITOR
    [MenuItem("Assets/Create/Weapon")]
    public static void CreateWeaponData()
    {
        string path = EditorUtility.SaveFilePanelInProject("SaveWeapon", "NewWeapon", "Asset", "SaveWeapon", "Assets/resources/Data/CharacterData/WeaponData");
        if (path == "")
            return;
        WeaponData weaponInstance = ScriptableObject.CreateInstance<WeaponData>();
        CharacterSkillData basicSkillInstance = ScriptableObject.CreateInstance<CharacterSkillData>();
        basicSkillInstance.Init(weaponInstance);
        weaponInstance.attacks[AttackType.Basic] = basicSkillInstance;

        CharacterSkillData dashSkillInstance = ScriptableObject.CreateInstance<CharacterSkillData>();
        dashSkillInstance.Init(weaponInstance);
        weaponInstance.attacks[AttackType.Dash] = dashSkillInstance;

        CharacterSkillData blockingSkillInstance = ScriptableObject.CreateInstance<CharacterSkillData>();
        blockingSkillInstance.Init(weaponInstance);
        weaponInstance.attacks[AttackType.Blocking] = blockingSkillInstance;

        CharacterSkillData chargeSkillInstance = ScriptableObject.CreateInstance<CharacterSkillData>();
        chargeSkillInstance.Init(weaponInstance);
        weaponInstance.attacks[AttackType.Charge] = chargeSkillInstance;

        CharacterSkillData criticalSkillInstance = ScriptableObject.CreateInstance<CharacterSkillData>();
        criticalSkillInstance.Init(weaponInstance);
        weaponInstance.attacks[AttackType.Critical] = criticalSkillInstance;

        AssetDatabase.CreateAsset(weaponInstance, path);
        int index = path.IndexOf(".");
        if (index > 0)
            path = path.Substring(0, index);
        AssetDatabase.CreateAsset(basicSkillInstance, path + "BasicSkillData.Asset");
        AssetDatabase.CreateAsset(dashSkillInstance, path + "DashSkillData.Asset");
        AssetDatabase.CreateAsset(blockingSkillInstance, path + "BlockingSkillData.Asset");
        AssetDatabase.CreateAsset(chargeSkillInstance, path + "ChargeSkillData.Asset");
        AssetDatabase.CreateAsset(criticalSkillInstance, path + "CriticalSkillData.Asset");
    }
#endif
}