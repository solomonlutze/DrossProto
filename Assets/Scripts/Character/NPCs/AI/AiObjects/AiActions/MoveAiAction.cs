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
    controller.lineToTargetIsClear = PathfindingSystem.Instance.IsPathClearOfHazards_SquareGrid(
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

        // Debug.Log("colliderCenterWorldSpace: " + colliderCenterWorldSpace);
        Debug.Log("minDistance: " + controller.minDistanceFromPathNode);
        Debug.Log("distance < minDistance: " + (Vector2.Distance(nextNodeLocation, colliderCenterWorldSpace) < controller.minDistanceFromPathNode));
        // Debug.Log("movementInput: " + movementInput);
        Debug.Log("coords from path: " + controller.pathToTarget[0].loc.tilemapCoordinates + "; coords from nextNodeLocation " + new TileLocation(nextNodeLocation, controller.currentFloor).tilemapCoordinates);
        GridManager.Instance.DEBUGHighlightTile(controller.pathToTarget[0].loc, Color.green);
        if (Vector2.Distance(nextNodeLocation, colliderCenterWorldSpace) < controller.minDistanceFromPathNode)
        {

          Debug.Log("next node " + controller.pathToTarget[0].loc.worldPosition);
          GridManager.Instance.DEBUGHighlightTile(controller.pathToTarget[0].loc, Color.magenta);
          controller.pathToTarget.RemoveAt(0);
          Debug.Log("removed! new next node " + controller.pathToTarget[0].loc.worldPosition);
          GridManager.Instance.DEBUGHighlightTile(controller.pathToTarget[0].loc, Color.cyan);
        }
      }
    }
    if (controller.pathToTarget == null && !controller.IsCalculatingPath())
    {
    }
    controller.SetMoveInput(movementInput);
  }
}