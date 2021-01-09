using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/IsBlocking")]
public class IsBlockingAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {
    return controller.blocking;
  }

}