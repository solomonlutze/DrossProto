using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Should be passed a SkillData object
// Populate SkillName and Description according to SkillData
public class SkillInfo : MonoBehaviour
{

  public CharacterSkillData skillData;
  public CharacterSkillData pupaSkillData;

  public SuperTextMesh skillNameText;
  public SuperTextMesh pupaSkillNameText;
  public int defaultFontSize;
  public Color32 defaultFontColor;

  void Start()
  {
    defaultFontSize = pupaSkillNameText.quality;
    defaultFontColor = pupaSkillNameText.color;
  }

  public void Init(CharacterSkillData data, CharacterSkillData pupaData, int addOrRemove)
  {
    skillNameText.text = data != null ? data.displayName : "";
    pupaSkillNameText.text = pupaData != null ? pupaData.displayName : "";
    if (addOrRemove > 0)
    {
      pupaSkillNameText.color = Color.red;
    }
    else if (addOrRemove < 0)
    {
      pupaSkillNameText.color = Color.blue;
    }
    else
    {
      pupaSkillNameText.color = Color.white;
    }
  }
}
