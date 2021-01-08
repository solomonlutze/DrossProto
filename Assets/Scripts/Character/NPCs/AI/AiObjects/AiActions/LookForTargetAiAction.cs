using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAi/Actions/LookForTarget")]
public class LookForTargetAiAction : AiAction
{
    public override void Act(AiStateController controller)
    {
        // PlayerController player = GameMaster.Instance.GetPlayerController();
        // if (player != null && Vector2.Distance(player.transform.position, controller.transform.position) < controller.detectionRange)
        // {
        //   controller.StartCalculatingPath(player.GetTileLocation(), this, player);
        // }
    }
}