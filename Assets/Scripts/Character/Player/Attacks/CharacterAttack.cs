using UnityEngine;
using UnityEditor;

[System.Serializable]
public class CharacterAttack : ScriptableObject
{
  public Hitbox hitboxObject;
  public HitboxInfo hitboxInfo;
  public float attackSpeed;
  public float range;
  public float hitboxSize;
  public float cooldown;

#if UNITY_EDITOR
  // The following is a helper that adds a menu item to create an TraitItem Asset
  [MenuItem("Assets/Create/CharacterAttack")]
  public static void CreateCharacterAttack()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Character Attack", "New Character Attack", "Asset", "Save Character Attack", "Assets/resources/Data/CharacterData/AttackData");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<CharacterAttack>(), path);
  }
#endif
}
