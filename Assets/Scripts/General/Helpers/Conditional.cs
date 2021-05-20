
using UnityEngine;
using System.Collections.Generic;

public enum ConditionType { Default, TileType }

[System.Serializable]
public class Condition
{
  public ConditionType conditionType;
  TileTag _tileType;
}

[System.Serializable]
public class Conditional<T>
{
  public T value;
  public Condition[] conditions;

}