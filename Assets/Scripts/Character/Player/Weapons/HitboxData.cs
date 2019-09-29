using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public enum HitboxShape { Box, Circle, Spawn };

// TODO: Currently only used by trait-spawned attacks. Should also be used by weapon-spawned attacks.
public enum HitboxSpawnPoint { CenteredOnPlayer, CenteredOnCrosshair }

[Serializable]
public class HitboxData : ScriptableObject
{

  public string id;
  public string hitboxDescription;

  public GameObject hitboxPrefab;
  // should this hitbox follow our initializingTransform; if false, it just hangs out in worldspace
  public bool followInitializingTransform;
  public float duration;
  public HitboxShape hitboxShape;
  // used only for circle shape.
  public float radius;

  // Used only for box shape.
  public Vector2 size;
  public DamageData damageInfo;

#if UNITY_EDITOR
  // The following is a helper that adds a menu item to create an HitboxData
  // Asset
  [MenuItem("Assets/Create/AttackInfo/HitboxData")]
  public static void CreateHitboxData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Hitbox Info", "New Hitbox Data", "Asset", "Save Hitbox Data", "Assets/resources/Data/CharacterData/AttackData/Hitboxes");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<HitboxData>(), path);
  }
#endif
}

// DAMAGE


public enum DamageType
{
  Physical,
  Heat,
  Fungal,
  Cold,
  Acid
}
