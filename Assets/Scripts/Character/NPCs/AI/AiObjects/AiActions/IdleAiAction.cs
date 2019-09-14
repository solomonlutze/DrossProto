using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAi/Actions/Idle")]
public class IdleAiAction : AiAction
{
  public override void Act(AiStateController controller)
  {
    // Do nothing. Maybe spin about a little?
  }
}