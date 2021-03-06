using UnityEngine;

[CreateAssetMenu(menuName = "PluggableAi/Actions/Block")]
public class BlockAiAction : AiAction
{

  public float minBlockTime = .75f;
  public float maxBlockTime = 3.0f;
  public float minNonBlockTime = .75f;
  public float maxNonBlockTime = 3.0f;
  public override void Act(AiStateController controller)
  {
    controller.blockTimer -= Time.deltaTime;
    if (controller.blockTimer <= 0)
    {
      controller.blocking = !controller.blocking;
      if (controller.blocking)
      {
        controller.blockTimer = Random.Range(minBlockTime, maxBlockTime);
      }
      else
      {
        controller.blockTimer = Random.Range(minNonBlockTime, maxNonBlockTime);
      }
    }

  }

  public override bool OnEntry(AiStateController controller)
  {
    if (Random.value < .5f)
    {
      controller.blocking = true;
      controller.blockTimer = Random.Range(minBlockTime, maxBlockTime);
    }
    else
    {
      controller.blocking = false;
      controller.blockTimer = Random.Range(minNonBlockTime, maxNonBlockTime);
    }
    return true;
  }

  public override void OnExit(AiStateController controller)
  {
    controller.blocking = false;
  }
}