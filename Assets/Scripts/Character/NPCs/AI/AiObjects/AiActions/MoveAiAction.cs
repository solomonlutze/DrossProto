using UnityEngine;

// Extend, don't instantiate
public class MoveAiAction : AiAction
{
  public int hazardCrossingCost = -1;

  [Tooltip("angle between each step in movement option calculation, in degrees")]
  public int movementOptionsAngleInterval = 15;

  [Tooltip("Distance to project out each option radius to check for hazards")]
  public float movementOptionProjectRange = .5f;

  [Tooltip("max stamina * minimumRemainingStaminaProportion = the amount that has to be left after dashing")]
  float minimumRemainingStaminaProportionForDash = .4f;
  float maxAngleFromDashTarget = 5f;
  // public override bool Decide(AiStateController controller)
  // {
  //     // if dashing keeps us above the stamina % threshold,
  //     // and we can dash without hitting anything or getting hurt,
  //     // and we wouldn't overshoot our destination,
  //     // we should dash!
  //     if (controller.overrideDestination != null
  //     && (
  //       controller.transform.position - controller.overrideDestination.transform.position).sqrMagnitude
  //       >= Mathf.Pow(controller.GetStat(CharacterStat.DashDistance), 2)
  //       )
  //     {
  //         return true;
  //     }
  //     return false;
  // }

  public override void Act(AiStateController controller)
  {
    // Do nothing
  }

  protected void MoveTowardsObjectOfInterest(AiStateController controller)
  {
    Vector2 movementInput = Vector2.zero;

    // MoveLocally(controller, GameMaster.Instance.GetPlayerController().transform.position);
    // return;
    WorldObject targetWorldLocation;
    if (controller.overrideDestination != null)
    {
      targetWorldLocation = controller.overrideDestination;
    }
    else if (controller.objectOfInterest != null)
    {
      targetWorldLocation = controller.objectOfInterest;
    }
    else if (controller.wanderDestination != null)
    {
      targetWorldLocation = controller.wanderDestination;
    }
    else
    {
      return;
    }

    controller.lineToTargetIsClear = PathfindingSystem.Instance.IsPathClearOfHazards(
      targetWorldLocation.transform.position,
      targetWorldLocation.GetFloorLayer(),
      controller
    );
    if (controller.lineToTargetIsClear)
    {
      float distanceFromTarget = CustomPhysicsController.GetMinimumDistanceBetweenObjects(targetWorldLocation.gameObject, controller.gameObject);

      if ((distanceFromTarget + .3f) > controller.minDistanceFromTarget)
      {
        movementInput = (targetWorldLocation.transform.position - controller.transform.position).normalized;
        // MaybeDash(controller, targetWorldLocation);
      }
    }
    else
    {
      controller.StartCalculatingPath(targetWorldLocation.GetTileLocation(), this);
      if (controller.pathToTarget != null && controller.pathToTarget.Count > 0)
      {
        Vector3 nextNodeLocation = new Vector3(controller.pathToTarget[0].loc.cellCenterWorldPosition.x, controller.pathToTarget[0].loc.cellCenterWorldPosition.y, controller.pathToTarget[0].loc.worldPosition.z);
        Vector3 colliderCenterWorldSpace = controller.transform.TransformPoint(controller.circleCollider.offset);
        movementInput = (nextNodeLocation - colliderCenterWorldSpace).normalized;
        Debug.DrawLine(nextNodeLocation, colliderCenterWorldSpace, Color.magenta, .1f, true);
        if (Vector2.Distance(nextNodeLocation, colliderCenterWorldSpace) < controller.minDistanceFromPathNode)
        {
          controller.pathToTarget.RemoveAt(0);
          while (controller.pathToTarget.Count > 2 && PathfindingSystem.Instance.IsPathClearOfHazards(
            controller.pathToTarget[1].loc.cellCenterWorldPosition,
            targetWorldLocation.GetFloorLayer(),
            controller
          ))
          {
            controller.pathToTarget.RemoveAt(0);
          }
        }
      }
    }
    controller.SetMoveInput(movementInput);
  }


  public void MoveLocally(AiStateController controller, Vector3 targetWorldLocation)
  {
    CalculateWeightedMovementOptions(controller, targetWorldLocation);
  }

  void CalculateWeightedMovementOptions(AiStateController controller, Vector3 targetPosition)
  {
    int angle = 0;
    Vector2 bestFitInput = Vector2.zero;
    float bestFitWeight = 0;
    Vector3 towardsTarget = targetPosition - controller.transform.position;
    Vector3 desiredAngle = Quaternion.AngleAxis(controller.aiSettings.localMovementWeights[0].movementAngles[0], Vector3.forward) * towardsTarget;
    Debug.DrawLine(desiredAngle.normalized * 1.25f + controller.transform.position, controller.transform.position, Color.cyan);
    while (angle < 360)
    {
      Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
      angle += movementOptionsAngleInterval;
      Vector3 possibleMovementDirection = rot * desiredAngle;
      if (!PathfindingSystem.Instance.IsPathClearOfHazards(controller.transform.position + possibleMovementDirection.normalized * movementOptionProjectRange, controller.currentFloor, controller))
      {
        // Debug.DrawLine(controller.transform.position, controller.transform.position + possibleMovementDirection.normalized * .5f, Color.red);
        continue;
      }
      float dotNormalized = (Vector3.Dot(desiredAngle.normalized, possibleMovementDirection.normalized) + 1) / 2;
      // Debug.DrawLine(controller.transform.position, controller.transform.position + possibleMovementDirection.normalized * dotNormalized, Color.green);
      if (dotNormalized > bestFitWeight)
      {
        bestFitInput = new Vector2(possibleMovementDirection.normalized.x, possibleMovementDirection.normalized.y);
        bestFitWeight = dotNormalized;
      }
    }
    controller.SetMoveInput(bestFitInput);

  }
  void MaybeDash(AiStateController controller, WorldObject targetWorldLocation)
  {
    // if dashing keeps us above the stamina % threshold,
    // and we can dash without hitting anything or getting hurt,
    // and we wouldn't overshoot our destination,
    // we should dash!
    if (
      DashWontOvershootTarget(controller, targetWorldLocation)
      && RemainingStaminaAfterDashAboveThreshold(controller)
      && CorrectFacingDirectionForDash(controller, targetWorldLocation)
      )
    {
      // dash
      controller.Dash();
    }
    // no dash
  }

  bool DashWontOvershootTarget(AiStateController controller, WorldObject targetWorldLocation)
  {
    return
        (controller.transform.position - targetWorldLocation.transform.position).sqrMagnitude
        >= Mathf.Pow(controller.GetStat(CharacterStat.DashDistance) + controller.GetMinPreferredAttackRange(), 2);
  }
  bool RemainingStaminaAfterDashAboveThreshold(AiStateController controller)
  {
    return (
      controller.GetCharacterVital(CharacterVital.CurrentStamina) - controller.GetDashStaminaCost() > controller.GetStat(CharacterStat.Stamina) * minimumRemainingStaminaProportionForDash
    );
  }
  bool CorrectFacingDirectionForDash(AiStateController controller, WorldObject targetWorldLocation)
  {
    return controller.GetAngleToDirection(targetWorldLocation.transform.position) < maxAngleFromDashTarget;
  }
  // Set destination and try to reach it.
  // Should be used with TargetIsReachable to fall out of state if target can't be reached.
  public override bool OnEntry(AiStateController controller)
  {
    // Debug.Log("moveAiAction onEntry");
    if (controller.objectOfInterest != null)
    {
      controller.StartCalculatingPath(controller.objectOfInterest.GetTileLocation(), this, controller.objectOfInterest);
      return true;
    }
    return false;
  }
}