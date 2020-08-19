using UnityEngine;
using System.Collections;

[System.Serializable]
public class CharacterSkillData : ScriptableObject
{

  public string displayName;
  [TextArea]
  public string description;
  public float ai_preferredMinRange; // closer than this and we'd like to back up
  public float ai_preferredAttackRangeBuffer; // weapon effectiveRange minus range buffer = ideal attack spot
  public virtual void Init(Character owner)
  {
  }

  public virtual IEnumerator UseSkill(Character owner)
  {
    yield return null;
  }

  public virtual IEnumerator PerformSkillCycle(Character owner)
  {
    yield return null;
  }
}