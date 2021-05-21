
using UnityEngine;
using System.Collections.Generic;

public enum ConditionType { TileType }

[System.Serializable]
public class Condition
{
  public ConditionType conditionType;
  public TileTag _tileType;
}

// NOTE: Conditionals and conditions will look uggo if you use them by themselves.
// So........ don't!!
[System.Serializable]
public class Conditional<T>
{
  public T value;
  public Condition[] conditions;

  // return true iff all conditions met
  public bool ConditionsMet(Character c)
  {
    foreach (Condition condition in conditions)
    {
      switch (condition.conditionType)
      {
        case ConditionType.TileType:
          if (!c.currentTile.groundTileTags.Contains(condition._tileType))
          {
            return false;
          }
          break;
        default:
          return false;
      }
    }
    return true;
  }

}