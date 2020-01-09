using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class DamageData_OLD : ScriptableObject
{

  public DamageType damageType;
  // amount of damage we do
  public float damageAmount;
  // amount of impulse force applied on hit

  public float knockback;
  // Duration in seconds of stun. TODO: should this value represent something else?
  public float stun;
  // amount of invulnerability this attack imparts. TODO: separate windows per damage source?
  public float invulnerabilityWindow;
  // whether this damage object should respect invulnerability from OTHER targets
  public bool ignoreInvulnerability;
  // whether this damage object should corrode certain tiles
  public bool corrosive = false;

  // whether this damage object should force hit enemies to drop their items
  public bool forcesItemDrop = false;

  public TileDurability durabilityDamageLevel = TileDurability.Delicate;

#if UNITY_EDITOR
  // The following is a helper that adds a menu item to create an DamageData
  // Asset
  [MenuItem("Assets/Create/AttackInfo/DamageData")]
  public static void CreateDamageData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Damage Info", "New Damage Data", "Asset", "Save Damage Data", "Assets/resources/Data/CharacterData/AttackData/Damage");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<DamageData_OLD>(), path);
  }
#endif

}
