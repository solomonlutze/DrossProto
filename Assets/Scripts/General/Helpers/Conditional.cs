
using UnityEngine;
using System.Collections.Generic;

public enum ConditionType { TileType, TouchingTileWithTag }

[System.Serializable]
public class Condition
{
  public ConditionType conditionType;
  public TileTag _tileType;
  public TileTag _touchingTileType;
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
          Debug.Log("evaluating tiletype condition; groundTileTags contains " + condition._tileType + ": " + c.currentTile.groundTileTags.Contains(condition._tileType));
          if (!c.currentTile.groundTileTags.Contains(condition._tileType))
          {
            return false;
          }
          break;
        case ConditionType.TouchingTileWithTag:
          if (!c.TouchingTileWithTag(condition._touchingTileType))
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