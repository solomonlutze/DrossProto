using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAi/Actions/OrientTowards")]
public class OrientTowardsAiAction : AiAction
{
  public override void Act(AiStateController controller)
  {
    OrientTowards(controller);
  }
  // look where you're going
  // if you're not going anywhere, look at object of interest
  // if there isn't one, don't bother with this stuff
  void OrientTowards(AiStateController controller)
  {
    controller.orientTowards = new Vector3(controller.GetMovementInput().x, controller.GetMovementInput().y, 0) + controller.transform.position;
    if (controller.GetMovementInput() == Vector2.zero && controller.objectOfInterest != null)
    {
      controller.orientTowards = controller.objectOfInterest.transform.position;
    }
    Debug.DrawLine(controller.transform.position, controller.orientTowards, Color.magenta);
  }
}