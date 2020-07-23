using UnityEngine;
using UnityEditor;
using System.Collections;

[System.Serializable]
public class StabAttack : AttackSkillData
{

#if UNITY_EDITOR
  // The following is a helper that adds a menu item to create an TraitItem Asset
  [MenuItem("Assets/Create/CharacterAttack/StabAttack")]
  public static void CreateCharacterAttack()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Stab Attack", "New Stab Attack", "Asset", "Save Stab Attack", "Assets/resources/Data/CharacterData/AttackData");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<StabAttack>(), path);
  }
#endif

  public override IEnumerator DoAttack(Character owner)
  {
    yield return new WaitForSeconds(attackDuration);
  }

}
