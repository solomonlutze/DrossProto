using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using ScriptableObjectArchitecture;

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
  public SkillRangeInfo[] skillRangeInfo;
  public SkillEffect[] skillEffects; // NOTE: Gotta fix this if we want vanilla skillEffects for anything!

  public virtual void Init(WeaponVariable weapon)
  {
    skillEffects = new SkillEffect[] {
      new SkillEffect()
    };
  }

  public virtual void BeginSkillEffect(Character owner)
  {
    skillEffects[owner.currentSkillEffectIndex].BeginSkillEffect(owner);
  }
  public virtual void UseSkill(Character owner)
  {
    SkillEffect currentSkillEffect = skillEffects[owner.currentSkillEffectIndex];
    currentSkillEffect.DoSkillEffect(owner);

    if (currentSkillEffect.useType == SkillEffectType.Continuous)
    {
      if (
        // || owner not holding button 
        currentSkillEffect.duration > 0 && owner.timeSpentInSkillEffect > currentSkillEffect.duration
        || !owner.receivingSkillInput
      )
      {
        owner.AdvanceSkillEffect();
      }
    }
    else if (owner.timeSpentInSkillEffect > skillEffects[owner.currentSkillEffectIndex].duration)
    {
      owner.AdvanceSkillEffect();
    }
  }
  public void CleanUp(Character owner)
  {
    // TO IMPLEMENT! should clean up weapons, at a minimum
  }

  public float GetActiveEffectDuration(Character owner)
  {
    return skillEffects[owner.currentSkillEffectIndex].duration;

  }
  public bool SkillIsInterruptable(Character owner)
  {

    return skillEffects[owner.currentSkillEffectIndex].interruptable;
  }
  public bool SkillMovesCharacter(Character owner)
  {
    return skillEffects[owner.currentSkillEffectIndex].properties.ContainsKey(SkillEffectProperty.Move);
  }

  public bool SkillHasMovementAbility(Character owner, CharacterMovementAbility movementAbility)
  {

    return skillEffects[owner.currentSkillEffectIndex].movementAbilities.Contains(movementAbility);
  }

  public float GetMovement(Character owner)
  {
    if (skillEffects[owner.currentSkillEffectIndex].properties.ContainsKey(SkillEffectProperty.Move))
    {
      return skillEffects[owner.currentSkillEffectIndex].properties[SkillEffectProperty.Move].Resolve(owner);
    }
    return 0;
  }

  public float GetMultiplierSkillProperty(Character owner, SkillEffectProperty property)
  {

    if (owner.currentSkillEffectIndex >= skillEffects.Length)
    {
      Debug.LogError("WARNING: tried to access skill property after skill should have ended");
      return 1;
    }
    if (skillEffects[owner.currentSkillEffectIndex].properties.ContainsKey(property))
    {
      return skillEffects[owner.currentSkillEffectIndex].properties[property].Resolve(owner);
    }
    return 1;
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