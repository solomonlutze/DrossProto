using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/TargetInRange")]
public class TargetInRangeAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {
    PlayerController player = GameMaster.Instance.GetPlayerController();
    if (TargetWithinSightRange(controller, player))
    {
      controller.objectOfInterest = player;
      return true;
    }
    return false;
  }

  bool TargetWithinSightRange(AiStateController controller, Character c)
  {
    return c != null && c.currentTile != null
      && Vector2.Distance(c.transform.position, controller.transform.position) < controller.GetSightRange() * DarkVisionAttributeData.GetVisibilityMultiplierForTile(controller.GetDarkVisionInfos(), c.currentTile);
  }
}