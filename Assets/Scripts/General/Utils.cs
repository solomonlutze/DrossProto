using System;
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

  public static Quaternion GetDirectionAngle(Vector3 targetPoint)
  {
    Vector2 target = targetPoint;
    float angle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
    return Quaternion.AngleAxis(angle, Vector3.forward);
  }

  public static float GetAngleToDirection(Quaternion rotation, Vector3 targetPoint)
  {
    return Quaternion.Angle(GetDirectionAngle(targetPoint), rotation);
  }
  static System.Random _R = new System.Random();
  public static T RandomEnumValue<T>()
  {
    var v = Enum.GetValues(typeof(T));
    return (T)v.GetValue(_R.Next(v.Length));
  }
  public static List<T> ShuffleEnum<T>()
  {
    List<T> values = new List<T>((T[])Enum.GetValues(typeof(T)));
    Utils.Shuffle<T>(values);
    return values;
  }

  public static void Shuffle<T>(List<T> list)
  {
    for (int i = 0; i < list.Count; i++)
    {
      T temp = list[i];
      int randomIndex = UnityEngine.Random.Range(i, list.Count);
      list[i] = list[randomIndex];
      list[randomIndex] = temp;
    }
  }
}
