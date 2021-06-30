using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "PluggableAi/LocalMovementWeight")]
public class AiLocalMovementWeight : ScriptableObject
{
  public int[] movementAngles;
  public AnimationCurve weightCurve;
}