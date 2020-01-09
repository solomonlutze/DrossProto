using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/EvadeAttack")]
public class EvadeAttackAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {
    float enemyAttackAngle = 30;
    if (
      controller.objectOfInterest != null
      && controller.objectOfInterest.GetComponent<Character>() != null
      && controller.objectOfInterest.GetComponent<Character>().GetAngleToDirection(controller.transform.position) < enemyAttackAngle)
    {
      Vector2 potentialOverrideDestination;
      if (controller.overrideDestination == null || !EvadeDestinationIsViable(controller, controller.overrideDestination.transform.position, enemyAttackAngle))
      {

        for (int i = 0; i < 10; i++)
        {
          potentialOverrideDestination = (Random.insideUnitSphere * Vector2.Distance(controller.objectOfInterest.transform.position, controller.transform.position))
          + controller.objectOfInterest.transform.position;
          if (EvadeDestinationIsViable(controller, potentialOverrideDestination, enemyAttackAngle))
          {
            controller.SetOverrideDestination(potentialOverrideDestination, controller.currentFloor);
            return true;
          }
        }
        // }
        // bool lineToTargetIsClear = PathfindingSystem.Instance.IsPathClearOfHazards(
        //   potentialOverrideDestination,
        //   controller.GetFloorLayer(),
        //   controller
        // );
        // if (
        //   lineToTargetIsClear
        //   && controller.objectOfInterest.GetComponent<Character>().GetAngleToDirection(potentialOverrideDestination) > enemyAttackAngle
        // )
        // {
        //   controller.SetOverrideDestination(potentialOverrideDestination, controller.currentFloor);
        //   return true;
        // }
      }
      else
      {
        // Debug.Log("we have a valid destination, or exceeded the number of attempts");
        return true;
      }
    }// && objectOfInterest && Vector2.Distance(controller.objectOfInterest.transform.position, controller.transform.position) < controller.GetAttackRange(controller.characterAttack, controller.attackModifiers)
     // && controller.GetAngleToTarget() < controller.attackAngleInDegrees)
    controller.UnsetOverrideDestination();
    return false;
  }

  bool EvadeDestinationIsViable(AiStateController controller, Vector2 potentialOverridePosition, float enemyAttackAngle)
  {
    bool lineToTargetIsClear = PathfindingSystem.Instance.IsPathClearOfHazards(
          potentialOverridePosition,
          controller.GetFloorLayer(),
          controller
        );
    return (
      lineToTargetIsClear
      && controller.objectOfInterest.GetComponent<Character>().GetAngleToDirection(potentialOverridePosition) > enemyAttackAngle
    );
  }
}