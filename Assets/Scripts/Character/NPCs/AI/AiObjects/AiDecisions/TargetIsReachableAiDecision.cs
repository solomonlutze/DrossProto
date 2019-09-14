using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/TargetIsReachable")]
public class TargetIsReachableAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {
    if (controller.CanReachObjectOfInterest())
    {
      return true;
    }
    controller.objectOfInterest = null;
    return false;
  }

}