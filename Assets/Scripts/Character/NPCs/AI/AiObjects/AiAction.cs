using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class AiAction : ScriptableObject
{
  public int hazardCrossingCost = -1;
  public abstract void Act(AiStateController controller);

  public virtual bool OnEntry(AiStateController controller)
  {
    return true;
  }
}