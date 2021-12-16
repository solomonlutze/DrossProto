using UnityEngine;

// Extend, don't instantiate
public class PathfindAiAction : AiAction
{
  public int hazardCrossingCost = -1;
  [Tooltip("max stamina * minimumRemainingStaminaProportion = the amount that has to be left after dashing")]
  float minimumRemainingStaminaProportionForDash = .4f;
  float maxAngleFromDashTarget = 5f;

  public override void Act(AiStateController controller)
  {
    // Do nothing
  }

  protected void PathfindTowardsObjectOfInterest(AiStateController controller)
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
    controller.StartCalculatingPath(targetWorldLocation.GetTileLocation(), this);
    if (controller.pathToTarget != null && controller.pathToTarget.Count > 0)
    {
      Node nextNode = controller.pathToTarget[0];
      Vector3 targetDirection = nextNode.loc.cellCenterWorldPosition - controller.transform.position;
      if (nextNode.activateSkill != null && Mathf.Abs(controller.GetAngleToDirection(targetDirection)) < 5)
      {
        controller.HandleSkillInput(nextNode.continueSkill);
      }
      Vector3 nextNodeLocation = new Vector3(nextNode.loc.cellCenterWorldPosition.x, nextNode.loc.cellCenterWorldPosition.y, nextNode.loc.z);
      Vector3 colliderCenterWorldSpace = controller.transform.TransformPoint(controller.physicsCollider.offset);
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
    controller.SetMoveInput(movementInput);
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
    Debug.Log("PathfindAiAction onEntry");
    if (controller.objectOfInterest != null)
    {
      controller.StartCalculatingPath(controller.objectOfInterest.GetTileLocation(), this, controller.objectOfInterest);
      return true;
    }
    return false;
  }
}