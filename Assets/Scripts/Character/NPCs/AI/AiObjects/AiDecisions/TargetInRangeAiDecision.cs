using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/TargetInRange")]
public class TargetInRangeAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {
    PlayerController player = GameMaster.Instance.GetPlayerController();
    if (player != null && Vector2.Distance(player.transform.position, controller.transform.position) < controller.GetSightRange())
    {
      controller.objectOfInterest = player;
      return true;
    }
    return false;
  }

}