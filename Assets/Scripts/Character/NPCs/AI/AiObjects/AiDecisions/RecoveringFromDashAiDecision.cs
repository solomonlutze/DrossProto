using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/RecoveringFromDash")]
public class RecoveringFromDashAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {
    // return controller.IsRecoveringFromDash();
    return false;
  }
}