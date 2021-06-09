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
  public SkillEffectSet[] skillEffectSets;
  // public SkillEffect[] skillEffects_old;

  public virtual void Init(WeaponVariable weapon)
  {
    // skillEffects_old = new SkillEffect[] {
    //   new SkillEffect()
    // };
  }

  public virtual void BeginSkillEffect(Character owner)
  {
    GetActiveSkillEffect(owner).BeginSkillEffect(owner);
  }

  public SkillEffectSet GetActiveSkillEffectSet(Character owner)
  {
    return skillEffectSets[owner.currentSkillEffectSetIndex];
  }

  public SkillEffect GetActiveSkillEffect(Character owner)
  {
    return GetActiveSkillEffectSet(owner).skillEffects[owner.currentSkillEffectIndex];
  }

  public virtual void UseSkill(Character owner)
  {
    SkillEffect currentSkillEffect = GetActiveSkillEffect(owner);
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
    else if (owner.timeSpentInSkillEffect > currentSkillEffect.duration)
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
    return GetActiveSkillEffect(owner).duration;
  }

  //NOTE: This is kind of weird.
  // a skill _EFFECT_ is advancable, but the effect _SET_ is what gets advanced.
  public bool CanAdvanceSkillEffectSet(Character owner)
  {
    return GetActiveSkillEffect(owner).advanceable && owner.currentSkillEffectSetIndex < skillEffectSets.Length - 1; //shouldn't need that last condition. don't mark the last skill effect advanceable!!
  }

  // a skill _EFFECT_ is interruptable, but the ENTIRE SKILL gets interrupted.
  public bool SkillIsInterruptable(Character owner)
  {
    return GetActiveSkillEffect(owner).interruptable;
  }
  public bool SkillMovesCharacter(Character owner)
  {
    return GetActiveSkillEffect(owner).movement.Count > 0;
  }
  public bool SkillMovesCharacterForward(Character owner)
  {
    return GetActiveSkillEffect(owner).movement.ContainsKey(SkillEffectCurveProperty.MoveForward);
  }
  public bool SkillMovesCharacterVertically(Character owner)
  {
    return GetActiveSkillEffect(owner).movement.ContainsKey(SkillEffectCurveProperty.MoveUp);
  }
  public bool SkillHasMovementAbility(Character owner, CharacterMovementAbility movementAbility)
  {
    return GetActiveSkillEffect(owner).movementAbilities.Contains(movementAbility);
  }

  public NormalizedCurve GetMovement(Character owner, SkillEffectCurveProperty movementProperty)
  {
    if (GetActiveSkillEffect(owner).movement.ContainsKey(movementProperty))
    {
      return GetActiveSkillEffect(owner).movement[movementProperty];
    }
    return null;
  }

  public float GetMultiplierSkillProperty(Character owner, SkillEffectFloatProperty property)
  {
    if (GetActiveSkillEffect(owner).properties.ContainsKey(property))
    {
      return GetActiveSkillEffect(owner).properties[property].Resolve(owner);
    }
    return 1;
  }

  public float GetEffectiveRange(Character owner)
  {
    List<float> effectRanges = new List<float>();
    foreach (SkillEffectSet effectSet in skillEffectSets)
    {
      foreach (SkillEffect effect in effectSet.skillEffects)
      {
        effectRanges.Add(effect.GetEffectiveRange(owner));
      }
    }
    return Mathf.Max(effectRanges.ToArray());
  }

  // public void CalculateRangeInfos(Character owner)
  // {
  //   List<SkillRangeInfo> rangeInfo = new List<SkillRangeInfo>();
  //   if (skillEffects_old.Length > 0)
  //   {
  //     rangeInfo = skillEffects_old[0].CalculateRangeInfos(owner);
  //   }
  //   skillRangeInfo = rangeInfo.ToArray();
  // }

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

  //  public void OnValidate()
  //  {
  //CalculateRangeInfos();
  // }

}