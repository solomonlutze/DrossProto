using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAi/Actions/Patrol")]
public class PatrolAiAction : PathfindAiAction
{
  public float patrolRadius = 1f;
  public override void Act(AiStateController controller)
  {
    if (controller.wanderDestination == null)
    {
      controller.StartValidateAndSetWanderDestination((Random.insideUnitCircle * patrolRadius) + controller.spawnLocation, controller.currentFloor, this);
    }
    PathfindTowardsObjectOfInterest(controller);
  }
}