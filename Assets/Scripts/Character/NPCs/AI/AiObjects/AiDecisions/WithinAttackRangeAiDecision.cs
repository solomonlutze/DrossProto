using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/WithinAttackRange")]
public class WithinAttackRangeAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {
    // Debug.Log("correct distance: " + (Vector2.Distance(controller.objectOfInterest.transform.position, controller.transform.position) < controller.GetMaxPreferredAttackRange()));
    // Debug.Log("correct angle: " + (controller.GetAngleToTarget() < controller.attackAngleInDegrees));
    if (
      controller.objectOfInterest != null
      && controller.WithinAttackRange(controller.objectOfInterest)
      // && Vector2.Distance(controller.objectOfInterest.transform.position, controller.transform.position) < controller.GetMaxPreferredAttackRange()
      && controller.WithinAttackAngle())
    {
      controller.WaitThenAttack();
      return true;
    }
    return false;
  }

}