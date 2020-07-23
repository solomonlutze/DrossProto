using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/NoTarget")]
public class NoTargetAiDecision : AiDecision
{
    public override bool Decide(AiStateController controller)
    {
        if (controller.overrideDestination == null)
        {
            return true;
        }
        return false;
    }

}