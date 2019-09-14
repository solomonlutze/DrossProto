using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/TimeInState")]
public class TimeInStateAiDecision : AiDecision
{
  public float allowedTimeInState;
  public override bool Decide(AiStateController controller)
  {
    return controller.timeSpentInState < allowedTimeInState;
  }

}