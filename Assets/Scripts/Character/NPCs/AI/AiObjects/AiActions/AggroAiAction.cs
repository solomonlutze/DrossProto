using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAi/Actions/Aggro")]
public class AggroAiAction : PathfindAiAction
{

  public override bool OnEntry(AiStateController controller)
  {
    base.OnEntry(controller);
    // Debug.Log("selected attack " + controller.selectedAttackType);
    // Debug.Break();
    return true;
  }

  public override void Act(AiStateController controller)
  {
    if (controller.objectOfInterest != null)
    {
      PathfindTowardsObjectOfInterest(controller);
    }
  }
}