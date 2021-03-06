using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAi/Actions/Attack")]
public class AttackAiAction : AiAction
{
  public override void Act(AiStateController controller)
  {
    controller.SetMoveInput(Vector2.zero);
  }
}