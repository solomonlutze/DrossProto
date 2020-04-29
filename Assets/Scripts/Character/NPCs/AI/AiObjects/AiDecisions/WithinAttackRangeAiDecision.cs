using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/WithinAttackRange")]
public class WithinAttackRangeAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {
    if (
      controller.objectOfInterest != null
      && Vector2.Distance(controller.objectOfInterest.transform.position, controller.transform.position) < controller.GetMaxPreferredAttackRange()
      && controller.GetAngleToTarget() < controller.attackAngleInDegrees)
    {
      controller.DoAttack((AttackSkillData)controller.GetSelectedCharacterSkill()); // todo: fix this????
      return true;
    }
    return false;
  }

}