using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAi/Actions/MoveLocal")]
public class MoveLocalAiAction : MoveAiAction
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
      MoveLocally(controller, controller.objectOfInterest.transform.position);
    }
  }
}