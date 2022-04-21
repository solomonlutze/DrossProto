using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "PluggableAi/Decisions/SkillReady")]
public class SkillReadyAiDecision : AiDecision
{

  public override bool Decide(AiStateController controller)
  {
    List<CharacterSkillData> viableAttacks = new List<CharacterSkillData>();
    foreach (CharacterSkillData skillData in controller.characterAttackSkills)
    {
      if (controller.CanUseSkill(skillData))
      {
        viableAttacks.Add(skillData);
      }
    }
    if (viableAttacks.Count > 0)
    {
      // choose skill
      controller.skillToUse = SelectAttack(controller, viableAttacks);
      Debug.Log("selected skill " + controller.skillToUse);
      return true;
    }
    // set skill to null
    controller.skillToUse = null;
    return false;
  }

  CharacterSkillData SelectAttack(AiStateController controller, List<CharacterSkillData> viableAttacks)
  {
    return viableAttacks[Random.Range(0, viableAttacks.Count)];
  }
}