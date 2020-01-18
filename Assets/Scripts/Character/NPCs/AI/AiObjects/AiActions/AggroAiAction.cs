using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAi/Actions/Aggro")]
public class AggroAiAction : MoveAiAction
{
    public override void Act(AiStateController controller)
    {
        if (
          controller.objectOfInterest != null
          // && Vector2.Distance(controller.objectOfInterest.transform.position, controller.transform.position) < controller.GetAttackRange(controller.characterAttack, controller.attackModifiers)
          && Vector2.Distance(controller.objectOfInterest.transform.position, controller.transform.position) >= controller.GetAttackRange(controller.characterAttack, controller.attackModifiers))
        {
            MoveTowardsObjectOfInterest(controller);
        }
    }
}