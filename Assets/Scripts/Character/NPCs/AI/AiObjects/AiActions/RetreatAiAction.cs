using UnityEngine;

// n.b. literally just for backing up. retreat != flee. please use this judiciously.

[CreateAssetMenu(menuName = "PluggableAi/Actions/Retreat")]
public class RetreatAiAction : MoveAiAction
{
    public override void Act(AiStateController controller)
    {
        if (controller.overrideDestination != null)
        {
            Debug.Log("Retreat???");
            MoveTowardsObjectOfInterest(controller);
        }
    }

    public override bool OnEntry(AiStateController controller)
    {
        Vector2 potentialOverrideDestination;
        float minRangeBuffer = .5f;
        // if (controller.overrideDestination == null)
        // {
        Vector3 heading = controller.transform.position - controller.objectOfInterest.transform.position;
        Vector3 normalizedHeading = heading / heading.magnitude;
        potentialOverrideDestination = normalizedHeading * (controller.GetMinPreferredAttackRange() + minRangeBuffer) + controller.transform.position;
        Debug.Log("potential override destination found; isViable: " + RetreatDestinationIsViable(controller, potentialOverrideDestination));
        if (RetreatDestinationIsViable(controller, potentialOverrideDestination))
        {
            Debug.Log("setting override destination!");
            controller.SetOverrideDestination(potentialOverrideDestination, controller.currentFloor);
            return true;
        }
        // }
        return false;
    }

    bool RetreatDestinationIsViable(AiStateController controller, Vector2 potentialOverridePosition)
    {
        bool lineToTargetIsClear = PathfindingSystem.Instance.IsPathClearOfHazards(
              potentialOverridePosition,
              controller.GetFloorLayer(),
              controller
            );
        Debug.DrawLine(controller.transform.position, potentialOverridePosition, Color.green, .1f);
        // bool positionWithinDesiredRange =
        //     Vector2.Distance(potentialOverridePosition, controller.objectOfInterest.transform.position) < controller.GetAttackRange()
        //     && Vector2.Distance(potentialOverridePosition, controller.objectOfInterest.transform.position) > controller.GetMinPreferredAttackRange();

        // Debug.Log("position within desired range: " + positionWithinDesiredRange);
        Debug.Log("checking if retreat destinatino is viable; lineToTargetIsClear: " + lineToTargetIsClear);
        return (
          lineToTargetIsClear
        //   && positionWithinDesiredRange
        );
    }

    //     // See if backing up will let us get to our desired distance
    // void SetNewPreferredSpot(AiStateController controller)
    // {
    //     Vector3 heading = controller.objectOfInterest.transform.position - controller.transform.position;
    //     Vector3 normalizedHeading = heading / heading.magnitude;
    //     Vector3 targetDestination = normalizedHeading * controller.GetAttackRange(controller.characterAttack, controller.attackModifiers);
    //     if (PathfindingSystem.Instance.IsPathClearOfHazards(
    //       targetDestination,
    //       controller.currentFloor,
    //       controller))
    //     {
    //         controller.SetOverrideDestination(targetDestination, controller.currentFloor);
    //     }
    //     controller.overrideDestination = null;
    // }
}