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
                Debug.Log("should be unsetting override destination");
                controller.overrideDestination = null;
                return true;
            }
        }
        if (controller.wanderDestination != null)
        {
            if (Vector3.Distance(controller.wanderDestination.transform.position, controller.transform.position) < minDistanceFromTarget)
            {
                controller.wanderDestination = null;
                return true;
            }
        }
        return false;
    }

}