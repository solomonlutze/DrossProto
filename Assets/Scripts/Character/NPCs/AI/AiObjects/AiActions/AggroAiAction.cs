using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAi/Actions/Aggro")]
public class AggroAiAction : MoveAiAction
{

    public float basicAttackWeight = 1.0f;
    public float powerAttackWeight = 1.0f;
    public override bool OnEntry(AiStateController controller)
    {
        base.OnEntry(controller);
        float attackRoll = Random.Range(0f, basicAttackWeight + powerAttackWeight);
        if (attackRoll < basicAttackWeight)
        {
            controller.selectedAttackType = AttackType.Basic;
        }
        else
        {
            controller.selectedAttackType = AttackType.Charge;
        }
        Debug.Log("selected attack " + controller.selectedAttackType);
        return true;
    }

    public override void Act(AiStateController controller)
    {
        if (
          controller.objectOfInterest != null
          // && Vector2.Distance(controller.objectOfInterest.transform.position, controller.transform.position) < controller.GetAttackRange(controller.characterAttack, controller.attackModifiers)
          && Vector2.Distance(controller.objectOfInterest.transform.position, controller.transform.position) >= controller.GetAttackRange())
        {
            MoveTowardsObjectOfInterest(controller);
        }
    }
}