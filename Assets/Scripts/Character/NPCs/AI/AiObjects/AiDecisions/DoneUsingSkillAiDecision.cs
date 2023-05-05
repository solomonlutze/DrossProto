using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/DoneUsingSkill")]
public class DoneUsingSkillAiDecision : AiDecision
{
  public float maxTimeInState = 3.0f;
  public int maxTimesToUseSkill = 3;
  public override bool Decide(AiStateController controller)
  {
    // should end if the skill is null OR
    // should end if the skill repeats or is continuous AND we've been in state >3s
    // should continue otherwise

    //invert:
    // should 
    if (controller.activeSkill == null)
    {
      controller.pressingSkill = null;
      return true;
    }
    if ((controller.activeSkill.IsContinuous(controller) || controller.activeSkill.SkillEffectIsRepeatable(controller) || controller.timeSkillUsed >= 1) && controller.timeSpentInState > maxTimeInState)
    {
      controller.pressingSkill = null;
      return true;
    }
    return false;
  }
}