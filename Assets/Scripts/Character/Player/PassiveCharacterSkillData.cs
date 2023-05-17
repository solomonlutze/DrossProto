using UnityEngine;

[System.Serializable]
public class PassiveCharacterSkillData : ScriptableObject
{
  public string name;
  public string displayName;
  [TextArea]
  public string description;
  public SkillEffect[] passiveEffects;
}