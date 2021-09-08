using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAi/Actions/Evade")]
public class EvadeAiAction : PathfindAiAction
{
  public override void Act(AiStateController controller)
  {
    if (controller.overrideDestination != null)
    {
      PathfindTowardsObjectOfInterest(controller);
    }
  }

  public override bool OnEntry(AiStateController controller)
  {
    Debug.Log("OnEntry for Evade action - " + controller.gameObject.name);
    base.OnEntry(controller);
    // int enemyAttackAngle = controller.objectOfInterest.GetComponent<Character>().GetAttackRadiusInDegrees();
    int enemyAttackAngle = 30;// controller.objectOfInterest.GetComponent<Character>().GetAttackRadiusInDegrees();
    Vector2 potentialOverrideDestination;
    if (controller.overrideDestination == null || !EvadeDestinationIsViable(controller, controller.overrideDestination.transform.position, enemyAttackAngle))
    {
      for (int i = 0; i < 100; i++)
      {
        potentialOverrideDestination = (Random.insideUnitSphere * Vector2.Distance(controller.objectOfInterest.transform.position, controller.transform.position))
        + controller.objectOfInterest.transform.position;
        if (EvadeDestinationIsViable(controller, potentialOverrideDestination, enemyAttackAngle))
        {
          controller.DelayThenSetOverrideDestination(potentialOverrideDestination, controller.currentFloor);
          return true;
        }
      }
    }
    return false;
  }

  bool EvadeDestinationIsViable(AiStateController controller, Vector2 potentialOverridePosition, float enemyAttackAngle)
  {
    bool lineToTargetIsClear = PathfindingSystem.Instance.IsPathClearOfHazards_SquareGrid(
          potentialOverridePosition,
          controller.GetFloorLayer(),
          controller
        );
    return (
      lineToTargetIsClear
      && controller.objectOfInterest.GetComponent<Character>().GetAngleToDirection(potentialOverridePosition) > enemyAttackAngle / 2
    );
  }
}