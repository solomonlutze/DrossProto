using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/CanUseBlockingAttackAiDecision")]
public class CanUseBlockingAttackAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {
    // Debug.Log("correct distance: " + (Vector2.Distance(controller.objectOfInterest.transform.position, controller.transform.position) < controller.GetMaxPreferredAttackRange()));
    // Debug.Log("correct angle: " + (controller.GetAngleToTarget() < controller.attackAngleInDegrees));
    if (
      controller.objectOfInterest != null
      && Vector2.Distance(controller.objectOfInterest.transform.position, controller.transform.position) < controller.GetMaxPreferredAttackRange(AttackType.Blocking)
      && controller.GetAngleToTarget() < controller.attackAngleInDegrees)
    {
      controller.WaitThenAttack();
      return true;
    }
    return false;
  }

}