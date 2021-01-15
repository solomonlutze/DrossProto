using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAi/Actions/Aggro")]
public class AggroAiAction : MoveAiAction
{

    public float basicAttackWeight = 1.0f;
    public float blockAttackWeight = 1.0f;
    public float dashAttackWeight = 1.0f;
    public float powerAttackWeight = 1.0f;
    public float critAttackWeight = 1.0f;
    public override bool OnEntry(AiStateController controller)
    {
        base.OnEntry(controller);
        float attackRoll = Random.Range(0f, basicAttackWeight + blockAttackWeight + dashAttackWeight + powerAttackWeight + critAttackWeight);
        // yes I can do this with an array, no I'm not going to
        if (attackRoll < basicAttackWeight)
        {
            controller.selectedAttackType = AttackType.Basic;
        }
        else if (attackRoll < basicAttackWeight + blockAttackWeight)
        {
            controller.selectedAttackType = AttackType.Blocking;
        }
        else if (attackRoll < basicAttackWeight + blockAttackWeight + dashAttackWeight)
        {
            controller.selectedAttackType = AttackType.Dash;
        }
        else if (attackRoll < basicAttackWeight + blockAttackWeight + dashAttackWeight + powerAttackWeight)
        {
            controller.selectedAttackType = AttackType.Charge;
        }
        else
        {
            controller.selectedAttackType = AttackType.Critical;
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