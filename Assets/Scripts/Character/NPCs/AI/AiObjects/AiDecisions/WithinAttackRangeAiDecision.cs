using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/WithinAttackRange")]
public class WithinAttackRangeAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {
    // Debug.Log("correct distance: " + (Vector2.Distance(controller.objectOfInterest.transform.position, controller.transform.position) < controller.GetMaxPreferredAttackRange()));
    if (
      controller.objectOfInterest != null
      && Vector2.Distance(controller.objectOfInterest.transform.position, controller.transform.position) < controller.GetMaxPreferredAttackRange()
      && controller.GetAngleToTarget() < controller.attackAngleInDegrees)
    {
      // Debug.Break(`);
      // Debug.Log("within range!!");
      controller.WaitThenAttack();
      // controller.UseAttack((AttackSkillData)controller.GetSelectedCharacterSkill()); // todo: fix this????
      return true;
    }
    return false;
  }

}