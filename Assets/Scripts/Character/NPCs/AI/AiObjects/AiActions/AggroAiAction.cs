using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAi/Actions/Aggro")]
public class AggroAiAction : MoveAiAction
{
  public override void Act(AiStateController controller)
  {
    MoveTowardsObjectOfInterest(controller);
  }
}