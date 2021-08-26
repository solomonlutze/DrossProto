
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Overrideable<T>
{
  public T defaultValue;
  [Tooltip("Overrides resolve from bottom to top")]
  public Conditional<T>[] overrides;
  public T Resolve(Character character = null)
  {
    if (character != null)
    {
      for (int i = overrides.Length - 1; i >= 0; i--)
      {
        if (overrides[i].ConditionsMet(character))
        {
          return overrides[i].value;
        }
      }
    }
    return defaultValue;
  }
  public T get(Character c)
  {
    return Resolve(c);
  }
  public Overrideable(T defaultVal)
  {
    defaultValue = defaultVal;
  }
}