using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/DoneAttacking")]
public class DoneAttackingAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {
    if (controller.usingSkill)
    {
      return false;
    }
    return true;
  }

}