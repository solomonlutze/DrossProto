using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/TargetIsReachable")]
public class TargetIsReachableAiDecision : AiDecision
{
    public override bool Decide(AiStateController controller)
    {
        Debug.Log("targetIsReachable decision");
        if (controller.CanReachObjectOfInterest() && controller.ObjectOfInterestWithinRangeOfSpawnPoint())
        {
            return true;
        }
        controller.objectOfInterest = null;
        return false;
    }

}