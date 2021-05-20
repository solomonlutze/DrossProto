
using UnityEngine;
using System.Collections.Generic;

public class Overrideable<T>
{
  Conditional<T>[] overrides;
  public T resolveValue(Character character)
  {
    foreach (Conditional<T> o in overrides)
    {
      foreach (Condition c in o.conditions)
      {
        if (c.conditionType == ConditionType.Default)
        {
          return o.value;
        }
      }
    }
    if (overrides.Length > 0)
    {
      return overrides[0].value;
    }
    return default(T);
  }
}