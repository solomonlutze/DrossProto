using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(menuName = "PluggableAi/AiSettings")]
public class AiSettings : ScriptableObject
{
  public AiLocalMovementWeight[] localMovementWeights;
  public float maxCombatDistance;
}