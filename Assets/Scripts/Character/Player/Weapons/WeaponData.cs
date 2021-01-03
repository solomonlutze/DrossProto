using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
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

  [SerializeField]
  public WeaponVariable weaponObject;
  // public Weapon weaponObject;
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
    WeaponVariable weapon = ScriptableObject.CreateInstance<WeaponVariable>();
    CharacterSkillData basicSkillInstance = ScriptableObject.CreateInstance<CharacterSkillData>();
    basicSkillInstance.Init(weapon);
    weaponInstance.attacks[AttackType.Basic] = basicSkillInstance;

    CharacterSkillData dashSkillInstance = ScriptableObject.CreateInstance<CharacterSkillData>();
    dashSkillInstance.Init(weapon);
    weaponInstance.attacks[AttackType.Dash] = dashSkillInstance;

    CharacterSkillData blockingSkillInstance = ScriptableObject.CreateInstance<CharacterSkillData>();
    blockingSkillInstance.Init(weapon);
    weaponInstance.attacks[AttackType.Blocking] = blockingSkillInstance;

    CharacterSkillData chargeSkillInstance = ScriptableObject.CreateInstance<CharacterSkillData>();
    chargeSkillInstance.Init(weapon);
    weaponInstance.attacks[AttackType.Charge] = chargeSkillInstance;

    CharacterSkillData criticalSkillInstance = ScriptableObject.CreateInstance<CharacterSkillData>();
    criticalSkillInstance.Init(weapon);
    weaponInstance.attacks[AttackType.Critical] = criticalSkillInstance;

    AssetDatabase.CreateAsset(weaponInstance, path);
    int index = path.IndexOf(".");
    if (index > 0)
      path = path.Substring(0, index);
    AssetDatabase.CreateAsset(weapon, path + "WeaponObject.Asset");
    AssetDatabase.CreateAsset(basicSkillInstance, path + "BasicSkillData.Asset");
    AssetDatabase.CreateAsset(dashSkillInstance, path + "DashSkillData.Asset");
    AssetDatabase.CreateAsset(blockingSkillInstance, path + "BlockingSkillData.Asset");
    AssetDatabase.CreateAsset(chargeSkillInstance, path + "ChargeSkillData.Asset");
    AssetDatabase.CreateAsset(criticalSkillInstance, path + "CriticalSkillData.Asset");
  }
#endif
}