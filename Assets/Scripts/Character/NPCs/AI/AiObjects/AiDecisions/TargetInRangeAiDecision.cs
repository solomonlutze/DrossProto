using UnityEngine;
[CreateAssetMenu(menuName = "PluggableAi/Decisions/TargetInRange")]
public class TargetInRangeAiDecision : AiDecision
{
  public override bool Decide(AiStateController controller)
  {
    PlayerController player = GameMaster.Instance.GetPlayerController();
    if (TargetWithinAttentionRange(controller, player))
    {
      controller.objectOfInterest = player;
      return true;
    }
    return false;
  }

  bool TargetWithinAttentionRange(AiStateController controller, Character c)
  {
    if (c == null || c.currentTile == null) { return false; }
    float distance = Vector2.Distance(c.transform.position, controller.transform.position);
    if (distance < controller.GetAwarenessRange()) // antennae overrides camouflage and darkvision; kinda broken for AI tbh!
    {
      return true;
    }
    float camouflageRange = c.GetCamouflageRange();
    if (camouflageRange > 0 && distance > camouflageRange)
    {
      return false;
    }
    return distance < controller.GetSightRange() * DarkVisionAttributeData.GetVisibilityMultiplierForTile(controller.GetDarkVisionInfos(), c.currentTile);
  }
}