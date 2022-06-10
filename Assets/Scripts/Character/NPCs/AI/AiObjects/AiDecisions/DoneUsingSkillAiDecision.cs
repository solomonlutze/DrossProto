using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/DoneUsingSkill")]
public class DoneUsingSkillAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {
    if (controller.activeSkill != null && controller.HasStaminaForSkill(controller.activeSkill))
    {

      return false;
    }
    controller.pressingSkill = null;
    return true;
  }

}