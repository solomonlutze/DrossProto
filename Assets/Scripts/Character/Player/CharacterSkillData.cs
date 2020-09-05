using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[System.Serializable]
public class SkillDelay
{
  public float duration;
  public float moveSpeedMultiplier = 0f;

  public float rotationSpeedMultiplier = 0f;
}

[System.Serializable]
public class CharacterSkillData : ScriptableObject
{

  public string displayName;
  [TextArea]
  public string description;
  public bool isAttack = false; // TODO: be better than this
  public float ai_preferredMinRange; // closer than this and we'd like to back up
  public float ai_preferredAttackRangeBuffer; // weapon effectiveRange minus range buffer = ideal attack spot
  public SkillDelay warmup;
  public SkillEffect[] skillEffects;
  public SkillDelay cooldown;
  public virtual void Init(Character owner)
  {
  }

  public virtual IEnumerator UseSkill(Character owner)
  {
    yield return new WaitForSeconds(warmup.duration);
    foreach (SkillEffect effect in skillEffects)
    {
      yield return effect.ActivateSkillEffect(owner);
    }
    yield return new WaitForSeconds(cooldown.duration);
  }
  public float GetEffectiveRange()
  {
    List<float> effectRanges = new List<float>();
    foreach (SkillEffect effect in skillEffects)
    {
      effectRanges.Add(effect.GetEffectiveRange());
    }
    return Mathf.Max(effectRanges.ToArray());
  }

  public virtual IEnumerator PerformSkillCycle(Character owner)
  {
    yield break;
  }

#if UNITY_EDITOR
  [MenuItem("Assets/Create/Skills/CharacterSkillData")]
  public static void CreateCharacterSkillData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Character Skill Data", "New Character Skill Data", "Asset", "Save Character Skill Data", "Assets/resources/Data/CharacterData/Skills/CharacterSkillData");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<CharacterSkillData>(), path);
  }
#endif
}