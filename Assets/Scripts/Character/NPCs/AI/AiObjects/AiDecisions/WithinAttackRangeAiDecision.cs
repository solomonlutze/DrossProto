using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/WithinAttackRange")]
public class WithinAttackRangeAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {
    if (controller.objectOfInterest == null)
    {

      // Debug.Log("no object of interest - exiting");
      return false;
    }
    List<CharacterSkillData> attacksInRange = GetAttacksInRange(controller);
    if (attacksInRange.Count > 0)
    {
      Debug.Log("within attack range!");
      CharacterSkillData attackToUse = attacksInRange[Random.Range(0, attacksInRange.Count)];
      // Debug.Log("selected attack " + attackToUse);
      controller.WaitThenAttack(attackToUse);
      return true;
    }
    return false;
  }

  public List<CharacterSkillData> GetAttacksInRange(AiStateController controller)
  {
    List<CharacterSkillData> viableAttacks = new List<CharacterSkillData>();
    Debug.Log("attack skills count: " + controller.characterAttackSkills.Count);
    foreach (CharacterSkillData skillData in controller.characterAttackSkills)
    {
      SkillRangeInfo[] rangeInfos = skillData.CalculateRangeInfosForSkillEffectSet(controller);
      Debug.Log("rangeInfos count " + rangeInfos.Length);
      // NOTE:
      // This "if" can evaluate to true if the character is in range (but not angle) for one attack spawn
      // and within angle (but not range) for another attack spawn
      // who knows if we care!
      if (
        !controller.UsingSkill()
        && controller.WithinAttackRange(controller.objectOfInterest, rangeInfos)
        && controller.WithinAttackAngle(controller.objectOfInterest, rangeInfos)
        && !controller.UsingMovementSkill())
      // && controller.HasSufficientStaminaForSelectedAttack())
      {
        // Debug.Log("adding viable attack " + skillData);
        viableAttacks.Add(skillData);
      }
    }
    return viableAttacks;
  }
}