using UnityEngine;
public abstract class AiDecision : ScriptableObject
{
  public abstract bool Decide(AiStateController controller);
}