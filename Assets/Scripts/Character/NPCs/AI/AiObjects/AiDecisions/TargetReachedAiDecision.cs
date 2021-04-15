using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/TargetReached")]
public class TargetReachedAiDecision : AiDecision
{
  public float minDistanceFromTarget;
  public override bool Decide(AiStateController controller)
  {
    // if (controller.overrideDestination == null && controller.wanderDestination == null) { return false; }
    if (controller.overrideDestination != null)
    {
      if (Vector3.Distance(controller.overrideDestination.transform.position, controller.transform.position) < minDistanceFromTarget)
      {
        controller.overrideDestination = null;
        return true;
      }
    }
    if (controller.wanderDestination != null)
    {
      if (controller.DEBUG_AiLogging)
      {
        Debug.Log("Destination: " + controller.wanderDestination.transform.position);
        Debug.Log("self: " + controller.transform.position);
        Debug.Log("min distance: " + minDistanceFromTarget);
        Debug.Log("distance from wander destination: " + Vector3.Distance(controller.wanderDestination.transform.position, controller.transform.position));
      }
      if (Vector3.Distance(controller.wanderDestination.transform.position, controller.transform.position) < minDistanceFromTarget)
      {
        controller.UnsetWanderDestination();
        return true;
      }
    }
    return false;
  }

}