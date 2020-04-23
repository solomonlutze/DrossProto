using UnityEngine;
using System.Collections;

[System.Serializable]
public class CharacterSkillData : ScriptableObject
{
  public virtual void Init(Character owner)
  {
  }

  public virtual IEnumerator UseSkill(Character owner)
  {
    yield return null;
  }
}