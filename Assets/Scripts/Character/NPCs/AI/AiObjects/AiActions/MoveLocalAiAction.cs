using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAi/Actions/MoveLocal")]
public class MoveLocalAiAction : AiAction
{

  [Tooltip("angle between each step in movement option calculation, in degrees")]
  public int movementOptionsAngleInterval = 15;

  [Tooltip("Distance to project out each option radius to check for hazards")]
  public float movementOptionProjectRange = .5f;

  public AiLocalMovementWeight[] movementWeightOverrides;

  public override bool OnEntry(AiStateController controller)
  {
    base.OnEntry(controller);
    return true;
  }

  public override void Act(AiStateController controller)
  {
    if (controller.objectOfInterest != null)
    {
      MoveLocally(controller, controller.objectOfInterest.transform.position);
    }
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
    (float, float) minAndMaxAttackRange = controller.GetMinAndMaxPreferredAttackRange();
    // Debug.Log("min range: " + minAndMaxAttackRange.Item1 + ", maxRange: " + minAndMaxAttackRange.Item2);
    float normalizedDistance = ((controller.transform.position - targetPosition).magnitude - minAndMaxAttackRange.Item1) / (minAndMaxAttackRange.Item2 - minAndMaxAttackRange.Item1);

    // Debug.Log("normalizedDistance: " + normalizedDistance + ", actual distance: " + (controller.transform.position - targetPosition).magnitude);
    float weightSum = 0;
    AiLocalMovementWeight[] weights = movementWeightOverrides.Length > 0 ? movementWeightOverrides : controller.aiSettings.localMovementWeights;
    foreach (AiLocalMovementWeight movementWeight in weights)
    {
      weightSum += movementWeight.weightCurve.Evaluate(normalizedDistance);
    }
    while (angle < 360)
    {
      Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
      angle += movementOptionsAngleInterval;
      Vector3 possibleMovementDirection = rot * towardsTarget;
      EnvironmentTileInfo tile = GridManager.Instance.GetTileAtWorldPosition(
            controller.transform.position + possibleMovementDirection.normalized * movementOptionProjectRange, controller.currentFloor
      );
      if (tile.IsEmpty() || !PathfindingSystem.Instance.CanPassOverTile(controller.transform.position.z, tile, controller))
      {
        continue;
      }
      float angleWeight = 0;
      foreach (AiLocalMovementWeight movementWeight in weights)
      {
        float maxNormalDot = 0;
        foreach (int movementAngle in movementWeight.movementAngles)
        {
          maxNormalDot = Mathf.Max(maxNormalDot, Vector3.Dot((Quaternion.AngleAxis(movementAngle, Vector3.forward) * towardsTarget).normalized, possibleMovementDirection.normalized) + 1) / 2;
        }
        angleWeight += maxNormalDot * movementWeight.weightCurve.Evaluate(normalizedDistance);
      }
      if (angleWeight > bestFitWeight)
      {
        bestFitInput = new Vector2(possibleMovementDirection.normalized.x, possibleMovementDirection.normalized.y);
        bestFitWeight = angleWeight;
      }

    }
    controller.SetMoveInput(bestFitInput);
  }
}