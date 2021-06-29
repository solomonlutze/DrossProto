using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/ClearPathToTarget")]
public class ClearPathToTargetAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {
    PlayerController player = GameMaster.Instance.GetPlayerController();
    if (player != null && controller.LineToTargetIsClear(player))
    {
      controller.objectOfInterest = player;
      return true;
    }
    return false;
    // if (controller.CanReachObjectOfInterest() && controller.ObjectOfInterestWithinRangeOfSpawnPoint())
    // {
    //   return true;
    // }
    // controller.objectOfInterest = null;
    // return false;
  }

}