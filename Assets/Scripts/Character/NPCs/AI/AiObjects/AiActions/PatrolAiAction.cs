using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAi/Actions/Patrol")]
public class PatrolAiAction : MoveAiAction
{
  public float patrolRadius = 1f;
  public override void Act(AiStateController controller)
  {
    if (controller.wanderDestination == null)
    {
      controller.StartValidateAndSetWanderDestination((Random.insideUnitCircle * patrolRadius) + controller.spawnLocation, controller.currentFloor);
    }
    MoveTowardsObjectOfInterest(controller);
  }
}