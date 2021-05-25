using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SkillEffectType { OneTime, Continuous }

[System.Serializable]
public class SkillEffectGroup
{
  SkillEffect[] skillEffects;
}

public enum SkillEffectProperty
{
  Move,
  MoveSpeed,
  TurnSpeed,
  AdjustStamina,

}

// [System.Serializable]
// public class SkillEffectProperty
// {
//   SkillEffectPropertyType property;
//   float value;
// }

[System.Serializable]
public class SkillEffect
{

  public SkillEffectType useType;
  public float duration;

  public SkillEffectPropertyToFloat properties;
  public AttackSpawn[] weaponSpawns;
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
  public virtual void DoSkillEffect(Character owner)
  {
    Debug.Log("activating skill effect " + owner.currentSkillEffectIndex + ", time: " + owner.timeSpentInSkillEffect);
    return;
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
