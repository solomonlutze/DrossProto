using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/CanUseBlockingAttackAiDecision")]
public class CanUseBlockingAttackAiDecision : AiDecision
{
    public override bool Decide(AiStateController controller)
    {
        if (
          controller.blocking
          && controller.objectOfInterest != null
          && Vector2.Distance(controller.objectOfInterest.transform.position, controller.transform.position) < controller.GetMaxPreferredAttackRange(AttackType.Blocking)
          && controller.GetAngleToTarget() < controller.attackAngleInDegrees)
        {
            controller.selectedAttackType = AttackType.Blocking;
            controller.WaitThenAttack();
            return true;
        }
        return false;
    }

}