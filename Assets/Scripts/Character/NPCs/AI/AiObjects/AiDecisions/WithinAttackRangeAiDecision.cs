using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAi/Decisions/WithinAttackRange")]
public class WithinAttackRangeAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {
    if (controller.objectOfInterest == null || controller.skillToUse == null)
    {
      return false;
    }
    SkillRangeInfo[] rangeInfos = controller.skillToUse.CalculateRangeInfosForSkillEffectSet(controller);
    if (controller.WithinAttackRangeAndAngle(controller.objectOfInterest, rangeInfos))
    {
      controller.HandleSkillInput(controller.skillToUse);
      return true;
    }
    return false;
  }

  // public List<CharacterSkillData> GetAttacksInRange(AiStateController controller)
  // {
  //   List<CharacterSkillData> viableAttacks = new List<CharacterSkillData>();
  //   // Debug.Log("attack skills count: " + controller.characterAttackSkills.Count);
  //   foreach (CharacterSkillData skillData in controller.characterAttackSkills)
  //   {
  //     SkillRangeInfo[] rangeInfos = skillData.CalculateRangeInfosForSkillEffectSet(controller);
  //     // Debug.Log("rangeInfos count " + rangeInfos.Length);
  //     // NOTE:
  //     // This "if" can evaluate to true if the character is in range (but not angle) for one attack spawn
  //     // and within angle (but not range) for another attack spawn
  //     // who knows if we care!
  //     if (
  //       !controller.UsingSkill()
  //       && controller.WithinAttackRange(controller.objectOfInterest, rangeInfos)
  //       && controller.WithinAttackAngle(controller.objectOfInterest, rangeInfos)
  //       && !controller.UsingMovementSkill())
  //     // && controller.HasSufficientStaminaForSelectedAttack())
  //     {
  //       // Debug.Log("adding viable attack " + skillData);
  //       viableAttacks.Add(skillData);
  //     }
  //   }
  //   return viableAttacks;
  // }
}