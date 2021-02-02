using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SkillEffectGroup
{
  SkillEffect[] skillEffects;
}

[System.Serializable]
public class SkillEffect
{
  // #if UNITY_EDITOR
  //   [MenuItem("Assets/Create/Skills/SkillEffect")]
  //   public static void CreateSkillEffect()
  //   {
  //     string path = EditorUtility.SaveFilePanelInProject("Save Skill Effect", "New Skill Effect", "Asset", "Save Skill Effect", "Assets/resources/Data/CharacterData/Skills/SkillEffects");
  //     if (path == "")
  //       return;
  //     AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<SkillEffect>(), path);
  //   }
  // #endif
  public SkillEffect()
  {

  }
  public virtual IEnumerator ActivateSkillEffect(Character owner)
  {
    Debug.Log("activating empty skill effect");
    yield break;
  }

  public virtual float GetEffectiveRange()
  {
    return 0f;
  }

  public virtual List<SkillRangeInfo> CalculateRangeInfos()
  {
    return new List<SkillRangeInfo>();
  }

  public virtual float CalculateDefaultStaminaCost()
  {
    return 0;
  }
}
