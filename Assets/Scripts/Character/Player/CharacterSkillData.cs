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

  public SkillRangeInfo(AttackSpawn spawn, Character owner)
  {
    minRange = spawn.range.get(owner);
    maxRange = spawn.range.get(owner) + spawn.weaponSize;
    minAngle = spawn.rotationOffset.get(owner);
    maxAngle = spawn.rotationOffset.get(owner);
  }
}

[System.Serializable]
public class CharacterSkillData : ScriptableObject
{

  public string displayName;
  [TextArea]
  public string description;
  [SerializeField]

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


  public virtual void EndSkillEffect(Character owner)
  {
    GetActiveSkillEffect(owner).EndSkillEffect(owner);
  }

  public SkillEffectSet GetActiveSkillEffectSet(Character owner)
  {
    return skillEffectSets[owner.currentSkillEffectSetIndex];
  }

  public SkillEffect GetActiveSkillEffect(Character owner)
  {
    return GetActiveSkillEffectSet(owner).skillEffects[owner.currentSkillEffectIndex];
  }

  public SkillEffect GetLastSkillEffect()
  {
    SkillEffectSet lastSet = GetLastSkillEffectSet();
    return lastSet.skillEffects[lastSet.skillEffects.Length - 1];
  }

  public SkillEffectSet GetLastSkillEffectSet()
  {
    return skillEffectSets[skillEffectSets.Length - 1];
  }
  public virtual void UseSkill(Character owner)
  {
    SkillEffect currentSkillEffect = GetActiveSkillEffect(owner);
    currentSkillEffect.DoSkillEffect(owner);

    if (currentSkillEffect.useType == SkillEffectType.Continuous)
    {
      if (
        !owner.pressingSkill == this && ((currentSkillEffect.minDuration.get(owner) > 0 && owner.timeSpentInSkillEffect > currentSkillEffect.minDuration.get(owner))
        || (currentSkillEffect.minDuration.get(owner) <= 0))
      )
      {
        owner.AdvanceSkillEffect();
      }
      else if (currentSkillEffect.maxDuration.get(owner) > 0 && owner.timeSpentInSkillEffect > currentSkillEffect.maxDuration.get(owner))
      {
        owner.AdvanceSkillEffect();
      }
    }
    else if (currentSkillEffect.useType == SkillEffectType.OneTime)
    {
      if (owner.timeSpentInSkillEffect > currentSkillEffect.minDuration.get(owner))
      {
        owner.AdvanceSkillEffect();
      }
    }

  }
  public void CleanUp(Character owner)
  {
    // TO IMPLEMENT! should clean up weapons, at a minimum
  }

  public float GetActiveEffectDuration(Character owner)
  {
    return GetActiveSkillEffect(owner).minDuration.get(owner);
  }
  public float GetActiveEffectMaxDuration(Character owner)
  {
    return GetActiveSkillEffect(owner).maxDuration.get(owner);
  }
  public bool GetShouldExecute(Character owner)
  {
    return GetActiveSkillEffect(owner).shouldExecute.Resolve(owner);
  }

  //NOTE: This is kind of weird.
  // a skill _EFFECT_ is advancable, but the effect _SET_ is what gets advanced.
  public bool CanAdvanceSkillEffectSet(Character owner)
  {
    return GetActiveSkillEffect(owner).advanceable.get(owner) && owner.currentSkillEffectSetIndex < skillEffectSets.Length - 1; //shouldn't need that last condition. don't mark the last skill effect advanceable!!
  }
  public bool IsContinuous(Character owner)
  {
    return GetActiveSkillEffect(owner).useType == SkillEffectType.Continuous;
  }

  public bool IsWhileAirborne(Character owner)
  {
    return GetActiveSkillEffect(owner).useType == SkillEffectType.WhileAirborne;
  }
  // a skill _EFFECT_ is interruptable, but the ENTIRE SKILL gets interrupted.
  public bool SkillIsInterruptable(Character owner)
  {
    return GetActiveSkillEffect(owner).interruptable.get(owner);
  }

  // Used only for pathfinding! Tells us that we can pivot from one use of the skill into the next.
  public bool SkillIsRepeatable()
  {
    return GetLastSkillEffect().cancelable.defaultValue;
  }

  // Used only for pathfinding! Tells us we should expect to be able to turn during this skill.
  public bool CanTurnDuringSkill()
  {
    foreach (SkillEffectSet set in skillEffectSets)
    {
      foreach (SkillEffect effect in set.skillEffects)
      {
        if (effect.movement.ContainsKey(SkillEffectMovementProperty.MoveForward) && effect.properties.ContainsKey(SkillEffectFloatProperty.RotationSpeed) && effect.properties[SkillEffectFloatProperty.RotationSpeed].defaultValue < 1f)
        {
          return false;
        }
      }
    }
    return true;
  }
  // a skill _EFFECT_ is cancelable, but the ENTIRE SKILL gets interrupted.
  public bool SkillIsCancelable(Character owner)
  {
    return GetActiveSkillEffect(owner).cancelable.get(owner);
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

  public bool SkillProvidesMovementAbility(CharacterMovementAbility movementAbility)
  {
    foreach (SkillEffectSet set in skillEffectSets)
    {
      foreach (SkillEffect effect in set.skillEffects)
      {
        if (effect.movementAbilities.Contains(movementAbility))
        {
          return true;
        }
      }
    }
    return false;
  }
  public float GetForwardMovementMagnitudeForPathfinding(EnvironmentTileInfo info)
  {
    float maxMagnitude = 0;
    foreach (SkillEffectSet effectSet in skillEffectSets)
    {
      foreach (SkillEffect effect in effectSet.skillEffects)
      {
        if (effect.movement.ContainsKey(SkillEffectMovementProperty.MoveForward))
        {
          maxMagnitude = Mathf.Max(maxMagnitude, effect.movement[SkillEffectMovementProperty.MoveForward].magnitude.Resolve(info));
        }
      }
    }
    return maxMagnitude;
  }

  public bool SkillCanMoveCharacterVertically()
  {
    foreach (SkillEffectSet effectSet in skillEffectSets)
    {
      foreach (SkillEffect effect in effectSet.skillEffects)
      {
        if (effect.movement.ContainsKey(SkillEffectMovementProperty.MoveUp))
        {
          return true;
        }
      }
    }
    return false;
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

  public float GetDamageMultiplierForType(Character owner, DamageType damageType)
  {
    if (GetActiveSkillEffect(owner).damageMultipliers.ContainsKey((SkillEffectDamageMultiplierProperty)damageType))
    {
      return GetActiveSkillEffect(owner).damageMultipliers[(SkillEffectDamageMultiplierProperty)damageType].Resolve(owner);
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

  public bool CanCrossEmptyTiles()
  {
    return SkillCanMoveCharacterVertically();
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