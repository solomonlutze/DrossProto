using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/InAttackCooldown")]
public class InAttackCooldownAiDecision : AiDecision
{

  // increment a counter
  // when counter exceeds controller attack cooldown,
  // we can attack!
  public override bool Decide(AiStateController controller)
  {
    return controller.attackCooldownTimer < controller.aiSettings.minAttackCooldown;
  }
}