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

  bool isAttack = false;
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
  public bool IsContinuous(Character owner)
  {
    return GetActiveSkillEffect(owner).useType == SkillEffectType.Continuous;
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
    return GetActiveSkillEffect(owner).movement.ContainsKey(SkillEffectMovementProperty.MoveForward);
  }
  public bool SkillMovesCharacterVertically(Character owner)
  {
    return GetActiveSkillEffect(owner).movement.ContainsKey(SkillEffectMovementProperty.MoveUp)
    && (GetActiveSkillEffect(owner).movement[SkillEffectMovementProperty.MoveUp].magnitude.Resolve(owner) > 0);
  }

  public bool SkillHasMovementAbility(Character owner, CharacterMovementAbility movementAbility)
  {
    return GetActiveSkillEffect(owner).movementAbilities.Contains(movementAbility);
  }

  public bool IsAttack()
  {
    return isAttack;
  }

  public NormalizedCurve GetMovement(Character owner, SkillEffectMovementProperty movementProperty)
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

  // We don't precalculate range info bc it may depend on character overrides
  // When determining whether to use an attack we should examine the range of that specific skill effect set
  // (the first set when deciding to use the attack, or the next set after the current one when deciding to continue a combo)
  public SkillRangeInfo[] CalculateRangeInfosForSkillEffectSet(Character owner, int skillEffectSetIdx = 0)
  {
    List<SkillRangeInfo> effectRangeInfos = new List<SkillRangeInfo>();
    if (skillEffectSetIdx < skillEffectSets.Length)
    {
      foreach (SkillEffect effect in skillEffectSets[skillEffectSetIdx].skillEffects)
      {
        effectRangeInfos.AddRange(effect.CalculateRangeInfos(owner));
      }
    }
    return effectRangeInfos.ToArray();
  }

  // This may not be useful but could be used to determine whether we're "close enough" in an abstract way,
  // vs specifically within range and angle of a particular attack effect.
  // Also tho it's probably broken
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

  void SetIsAttack()
  {
    if (skillEffectSets == null) { return; }
    foreach (SkillEffectSet set in skillEffectSets)
    {
      if (set.skillEffects == null) { continue; }
      foreach (SkillEffect effect in set.skillEffects)
      {
        if (effect.weaponSpawns.Length > 0)
        {
          isAttack = true;
          return;
        }
      }
    }
    isAttack = false;
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
    SetIsAttack();
  }

}