using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[System.Serializable]
public class CharacterAttackModifiersInfo : ScriptableObject
{
  public CharacterAttackModifiers attackModifiers;
#if UNITY_EDITOR
  // The following is a helper that adds a menu item to create an TraitItem Asset
  [MenuItem("Assets/Create/CharacterAttackModifiers")]
  public static void CreateCharacterAttackModifiers()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Attack Modifiers ", "New Attack Modifiers", "Asset", "Save Attack Modifiers", "Assets/resources/Data/CharacterData/AttackData/Modifiers");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<CharacterAttackModifiersInfo>(), path);
  }
#endif
}