using System;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
  public static float EaseOut(float t, float p)
  {
    return (float)(1f - Math.Pow(1 - t, p));
  }
  public static Dictionary<T, U> InitializeEnumDictionary<T, U>(bool populate = true) where U : new()
  {
    Dictionary<T, U> dictionary = new Dictionary<T, U>();
    foreach (T enumType in (T[])Enum.GetValues(typeof(T)))
    {
      U value = populate ? new U() : default(U);
      dictionary.Add(enumType, value);
    }
    return dictionary;
  }
}
