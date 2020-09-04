using UnityEngine;
using System.Collections;
using UnityEditor;

[System.Serializable]
public class SkillEffectGroup
{
  SkillEffect[] skillEffects;
}
[System.Serializable]
public class SkillEffect : ScriptableObject
{
#if UNITY_EDITOR
  [MenuItem("Assets/Create/Skills/SkillEffect")]
  public static void CreateSkillEffect()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Skill Effect", "New Skill Effect", "Asset", "Save Skill Effect", "Assets/resources/Data/CharacterData/Skills/SkillEffects");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<SkillEffect>(), path);
  }
#endif
  public virtual IEnumerator ActivateSkillEffect(Character owner)
  {
    Debug.Log("activating empty skill effect");
    yield break;
  }

  public virtual float GetEffectiveRange()
  {
    return 0f;
  }
}
