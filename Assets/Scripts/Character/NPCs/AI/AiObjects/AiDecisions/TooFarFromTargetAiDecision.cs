using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/TooFarFromTarget")]
public class TooFarFromTargetAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {
    if (
      controller.objectOfInterest != null
      && controller.TooFarFromTarget(controller.objectOfInterest)
        )
    {
      return true;
    }
    return false;
  }

}