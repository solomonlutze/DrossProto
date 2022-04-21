using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAi/Actions/QueueSkillFollowUp")]
public class QueueSkillFollowUpAiAction : AiAction
{
  // check if we are using a skill,
  // if the skill has a next effect,
  // and if we SHOULD use the skill's next effect (ie we are in range)
  // if so, queue it
  public override void Act(AiStateController controller)
  {
    if (
      controller.UsingSkill()
      && controller.HasStaminaForSkill(controller.activeSkill)
    )
    {

    }
  }
}