
using UnityEngine;
using System.Collections.Generic;

public enum ConditionType { TileType, TouchingTileWithTag, ChargeLevel }
public enum Comparator { Equals, LessThan, LessOrEqual, GreaterThan, GreaterOrEqual }

[System.Serializable]
public class Condition
{
  public Comparator comparator = Comparator.Equals;
  public ConditionType conditionType;
  public TileTag _tileType;
  public TileTag _touchingTileType;
  public int _chargeLevel;
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
        case ConditionType.ChargeLevel:
          if (!DoComparison(c.chargeLevel, condition._chargeLevel, condition.comparator))
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

  // Mostly for pathfinding. Accepts a tile the character will be on when using the skill.
  public bool ConditionsMet(EnvironmentTileInfo eti)
  {
    foreach (Condition condition in conditions)
    {
      switch (condition.conditionType)
      {
        case ConditionType.TileType:
          if (eti.groundTileTags.Contains(condition._tileType))
          {
            return false;
          }
          break;
        case ConditionType.TouchingTileWithTag:
          // TODO: evaluate adjacent tiles
          return false;
        // if (!c.TouchingTileWithTag(condition._touchingTileType))
        // {
        //   return false;
        // }
        // break;
        case ConditionType.ChargeLevel:
        // TODO: uhhhhhh
        // if (!DoComparison(c.chargeLevel, condition._chargeLevel, condition.comparator))
        // {
        //   return false;
        // }
        // break;
        default:
          return false;
      }
    }
    return true;
  }
  public bool DoComparison(float value, float target, Comparator comparator)
  {
    switch (comparator)
    {
      case Comparator.Equals:
        return target == value;
      case Comparator.LessThan:
        return value < target;
      case Comparator.LessOrEqual:
        return value <= target;
      case Comparator.GreaterThan:
        return value > target;
      case Comparator.GreaterOrEqual:
        return value >= target;
      default:
        return false;
    }
  }
}