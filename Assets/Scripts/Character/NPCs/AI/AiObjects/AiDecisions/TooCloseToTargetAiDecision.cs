using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/TooCloseToTarget")]
public class TooCloseToTargetAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {
    if (
      controller.objectOfInterest != null
      && controller.TooCloseToTarget(controller.objectOfInterest)
        )
    {
      return true;
    }
    return false;
  }

}