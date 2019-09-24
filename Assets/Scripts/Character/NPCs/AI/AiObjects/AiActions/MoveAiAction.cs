using UnityEngine;

// Extend, don't instantiate
public class MoveAiAction : AiAction
{
  public override void Act(AiStateController controller)
  {
    // Do nothing
  }

  protected void MoveTowardsObjectOfInterest(AiStateController controller)
  {
    Vector2 movementInput = Vector2.zero;
    WorldObject targetWorldLocation;
    if (controller.objectOfInterest != null)
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
      movementInput = (targetWorldLocation.transform.position - controller.transform.position).normalized;
    }
    else
    {
      controller.StartCalculatingPath(targetWorldLocation.GetTileLocation());
      if (controller.pathToTarget != null && controller.pathToTarget.Count > 0)
      {
        Vector3 nextNodeLocation = new Vector3(controller.pathToTarget[0].loc.position.x + .5f, controller.pathToTarget[0].loc.position.y + .5f, 0);
        Vector3 colliderCenterWorldSpace = controller.transform.TransformPoint(controller.col.offset);
        movementInput = (nextNodeLocation - colliderCenterWorldSpace).normalized;
        Debug.DrawLine(nextNodeLocation, colliderCenterWorldSpace, Color.red, .25f, true);
        if (Vector3.Distance(nextNodeLocation, colliderCenterWorldSpace) < controller.minDistanceFromPathNode)
        {
          controller.pathToTarget.RemoveAt(0);
        }
      }
    }
    controller.SetMoveInput(movementInput);
  }
}