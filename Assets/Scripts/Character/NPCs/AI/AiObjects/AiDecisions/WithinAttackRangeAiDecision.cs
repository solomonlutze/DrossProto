using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/WithinAttackRange")]
public class WithinAttackRangeAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {
    if (
      controller.objectOfInterest != null
      && Vector2.Distance(controller.objectOfInterest.transform.position, controller.transform.position) < controller.GetAttackRange(controller.characterAttack, controller.attackModifiers)
      && controller.GetAngleToTarget() < controller.attackAngleInDegrees)
    {
      // Debug.Log(controller.name + " Within attacking range");
      controller.Attack();
      return true;
    }
    return false;
  }

}