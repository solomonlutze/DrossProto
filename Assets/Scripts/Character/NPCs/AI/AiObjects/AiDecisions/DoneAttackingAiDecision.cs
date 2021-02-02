using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/DoneAttacking")]
public class DoneAttackingAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {
    if (controller.attacking)
    {
      return false;
    }
    return true;
  }

}