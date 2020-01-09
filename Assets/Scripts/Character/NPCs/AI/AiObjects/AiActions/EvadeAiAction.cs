using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAi/Actions/Evade")]
public class EvadeAiAction : MoveAiAction
{
  public override void Act(AiStateController controller)
  {
    MoveTowardsObjectOfInterest(controller);
  }
}