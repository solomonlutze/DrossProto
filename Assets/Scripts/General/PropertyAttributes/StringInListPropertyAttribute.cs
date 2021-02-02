using UnityEngine;
using System;

public class StringInList : PropertyAttribute
{
  public delegate string[] GetStringList();

  public StringInList(params string[] list)
  {
    List = list;
  }

  public StringInList(Type type, string methodName, object[] parameters = null)
  {
    var method = type.GetMethod(methodName);
    if (method != null)
    {
      Debug.Log("method: " + method + ", parameters: " + parameters.Length);
      List = method.Invoke(null, parameters) as string[];
    }
    else
    {
      Debug.LogError("NO SUCH METHOD " + methodName + " FOR " + type);
    }
  }

  public string[] List
  {
    get;
    private set;
  }
}