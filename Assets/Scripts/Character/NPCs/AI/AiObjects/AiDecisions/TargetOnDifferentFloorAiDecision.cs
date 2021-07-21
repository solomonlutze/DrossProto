using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/TargetOnDifferentFloor")]
public class TargetOnDifferentFloorAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {

    if (controller.objectOfInterest != null && controller.objectOfInterest.currentFloor == controller.currentFloor)
    {
      return false;
    }
    controller.objectOfInterest = null;
    return true;
  }

}