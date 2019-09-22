using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CharacterData : ScriptableObject
{

  public CharacterStatToFloatDictionary defaultStats;
  public DamageTypeToFloatDictionary damageTypeResistances;
  public CharacterAttackModifiers attackModifiers;
  public CharacterAttackModifiers dashAttackModifiers;
  public List<CharacterMovementAbility> movementAbilities;

  public void Awake()
  {
    if (defaultStats == null)
    {
      defaultStats = new CharacterStatToFloatDictionary();
    }
    if (damageTypeResistances == null)
    {
      damageTypeResistances = new DamageTypeToFloatDictionary();
    }
    if (attackModifiers == null)
    {
      attackModifiers = new CharacterAttackModifiers();
    }
    if (dashAttackModifiers == null)
    {
      attackModifiers = new CharacterAttackModifiers();
    }
    if (movementAbilities == null)
    {
      movementAbilities = new List<CharacterMovementAbility>();
    }
    Debug.Log("character data init'd!");
  }

#if UNITY_EDITOR
  // The following is a helper that adds a menu item to create an PlayerCharacterData Asset
  [MenuItem("Assets/Create/CharacterData/PlayerCharacterData")]
  public static void CreatePlayerCharacterData()
  {
    string path = EditorUtility.SaveFilePanelInProject(
        "Save Player Character Data",
        "New Player Character Data",
        "Asset",
        "Save Player Character Data",
        "Assets/resources/Data/CharacterData/PlayerCharacterData"
    );
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<CharacterData>(), path);
  }

  // The following is a helper that adds a menu item to create an PlayerCharacterData Asset
  [MenuItem("Assets/Create/CharacterData/NpcData")]
  public static void CreateNPCData()
  {
    string path = EditorUtility.SaveFilePanelInProject(
        "Save NPC Data",
        "New NPC Data",
        "Asset",
        "Save NPC Data",
        "Assets/resources/Data/CharacterData/NpcData"
    );
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<CharacterData>(), path);
  }
#endif
}