using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using ScriptableObjectArchitecture;

[System.Serializable]
public class SkillDelay
{
  public float duration;
  public float moveSpeedMultiplier = 0f;
  public float rotationSpeedMultiplier = 0f;
}

[System.Serializable]
public class SkillRangeInfo
{

  [Tooltip("Closest attack distance to the user. Usually the attackSkillEffect's Range.")]
  public float minRange;
  [Tooltip("Furthest attack distance from the user. Usually range + weapon size + any positive Move amount.")]
  public float maxRange;
  [Tooltip("Lowest angle of the attack, in degrees")]
  public float minAngle;
  [Tooltip("Highest angle of the attack, in degrees")]
  public float maxAngle;

  public SkillRangeInfo(AttackSpawn spawn)
  {
    minRange = spawn.range;
    maxRange = spawn.range + spawn.weaponSize;
    minAngle = spawn.rotationOffset;
    maxAngle = spawn.rotationOffset;
  }
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
  public AttackSkillEffect[] skillEffects; // NOTE: Gotta fix this if we want vanilla skillEffects for anything!

  public SkillRangeInfo[] skillRangeInfo;

  public SkillDelay cooldown;

  public virtual void Init(WeaponVariable weapon)
  {
    skillEffects = new AttackSkillEffect[] {
      new AttackSkillEffect(weapon)
    };
  }

  public virtual IEnumerator UseSkill(Character owner, bool skipWarmup = false)
  {
    if (!skipWarmup)
    {
      yield return new WaitForSeconds(warmup.duration);
    }
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

  public void CalculateRangeInfos()
  {
    Debug.Log("calculate range infos~");
    List<SkillRangeInfo> rangeInfo = new List<SkillRangeInfo>();
    if (skillEffects.Length > 0)
    {
      rangeInfo = skillEffects[0].CalculateRangeInfos();
    }
    skillRangeInfo = rangeInfo.ToArray();
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

  public void OnValidate()
  {
    CalculateRangeInfos();
  }
}