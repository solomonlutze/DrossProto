
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class OverrideableAttribute : PropertyAttribute
{
  // public T defaultValue;
  // [Tooltip("Overrides resolve from bottom to top")]
  // public Conditional<T>[] overrides;
  // public T Resolve(Character character)
  // {
  //   for (int i = overrides.Length - 1; i >= 0; i--)
  //   {
  //     if (overrides[i].ConditionsMet(character))
  //     {
  //       return overrides[i].value;
  //     }
  //   }
  //   return defaultValue;
  // }
  // public T get(Character c)
  // {
  //   return Resolve(c);
  // }
  public OverrideableAttribute(System.Type t) {

    Debug.Log("t: "+t);
    // Debug.Log("attribute: "+attribute);
    // Debug.Log("fieldInfo: "+fieldInfo);
  }
}