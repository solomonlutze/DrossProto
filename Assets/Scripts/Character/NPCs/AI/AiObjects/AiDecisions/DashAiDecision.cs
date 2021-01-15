using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/Dash")]
public class DashAiDecision : AiDecision
{
  [Tooltip("max stamina * minimumRemainingStaminaProportion = the amount that has to be left after dashing")]
  float minimumRemainingStaminaProportion = .4f;
  public override bool Decide(AiStateController controller)
  {
    // if dashing keeps us above the stamina % threshold,
    // and we can dash without hitting anything or getting hurt,
    // and we wouldn't overshoot our destination,
    // we should dash!
    if (controller.overrideDestination != null
    && (controller.transform.position - controller.overrideDestination.transform.position).sqrMagnitude >= controller.GetStat(CharacterStat.DashDistance) * *2)
    {

    }
  }

  // float GetEffectiveEnemyAttackAngle(AiStateController controller)
  // {
  //     return Mathf.Max(
  //         controller.objectOfInterest.GetComponent<Character>().GetAttackRadiusInDegrees() / 2,
  //         15
  //     );
  // }
}