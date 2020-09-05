using UnityEngine;
using System.Collections;
using UnityEditor;

[System.Serializable]
public class AnimaSpellData : SpellSkillData
{


#if UNITY_EDITOR
  // The following is a helper that adds a menu item to create an TraitItem Asset
  [MenuItem("Assets/Create/CharacterSpell/AnimaSpell")]
  public static void CreateCharacterSpell()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Anima Spell", "New Anima Spell", "Asset", "Save Anima Spell", "Assets/resources/Data/CharacterData/SpellData");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AnimaSpellData>(), path);
  }
#endif
  public override IEnumerator UseSkill(Character owner)
  {
    Debug.Log("casting spell " + name);
    yield break;
  }
}