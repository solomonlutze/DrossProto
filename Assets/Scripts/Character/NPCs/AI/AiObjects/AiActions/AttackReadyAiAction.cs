using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAi/Actions/AttackReady")]
public class AttackReadyAiAction : AiAction
{
  public override void Act(AiStateController controller)
  {

  }
  public override bool OnEntry(AiStateController controller)
  {
    controller.EnableAttackReadyIndicator();
    return true;
  }
}