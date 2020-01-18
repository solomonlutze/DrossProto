using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/EvadeAttack")]
public class EvadeAttackAiDecision : AiDecision
{
    public override bool Decide(AiStateController controller)
    {
        if (
          controller.objectOfInterest != null
          && controller.objectOfInterest.GetComponent<Character>() != null
          && controller.objectOfInterest.GetComponent<Character>().attacking
          && controller.objectOfInterest.GetComponent<Character>().GetAngleToDirection(controller.transform.position) < controller.objectOfInterest.GetComponent<Character>().GetAttackRadiusInDegrees() / 2)
        {
            Debug.Log("Deciding to invade!");
            return true;
        }
        controller.UnsetOverrideDestination();
        return false;
    }

    float GetEffectiveEnemyAttackAngle(AiStateController controller)
    {
        return Mathf.Max(
            controller.objectOfInterest.GetComponent<Character>().GetAttackRadiusInDegrees() / 2,
            15
        );
    }
}