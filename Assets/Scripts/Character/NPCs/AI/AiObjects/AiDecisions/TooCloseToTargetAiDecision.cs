using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/TooCloseToTarget")]
public class TooCloseToTargetAiDecision : AiDecision
{
    public override bool Decide(AiStateController controller)
    {
        if (
          controller.objectOfInterest != null
          && Vector2.Distance(controller.objectOfInterest.transform.position, controller.transform.position) < controller.GetMinPreferredAttackRange()
        )
        {
            return true;
        }
        return false;
    }

}