using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/TargetReached")]
public class TargetReachedAiDecision : AiDecision
{
  public float minDistanceFromTarget;
  public override bool Decide(AiStateController controller)
  {
    if (controller.wanderDestination == null) { return false; }
    if (Vector3.Distance(controller.wanderDestination.transform.position, controller.transform.position) < minDistanceFromTarget)
    {
      controller.wanderDestination = null;
      return true;
    }
    return false;
  }

}