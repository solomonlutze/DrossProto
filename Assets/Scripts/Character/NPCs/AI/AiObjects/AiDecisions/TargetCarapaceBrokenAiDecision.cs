using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/TargetCarapaceBroken")]
public class TargetCarapaceBrokenAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {
    Character targetCharacter = controller.objectOfInterest?.GetComponent<Character>();
    if (targetCharacter != null && targetCharacter.carapaceBroken)
    {
      controller.critTarget = targetCharacter;
      return true;
    }
    controller.critTarget = null;
    return false;
  }
}