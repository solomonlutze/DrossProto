
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class EditorHelpers {
  #if UNITY_EDITOR
  public static string[] AllItemsOfType(string itemType)
  {
    var temp = new List<string>();
    var path =  Application.dataPath + "/resources/Data/ItemData/"+itemType+"/";
    foreach (string file in System.IO.Directory.GetFiles(path))
    {
      string name = file.Substring(file.LastIndexOf('/')+1);
      if (name.Substring(name.Length - 4, 4) == "meta") { continue; }
      name = name.Substring(0,name.Length-6);
      Debug.Log("adding "+name);
      temp.Add(name);
    }
    return temp.ToArray();

  }

  public static string[] AllWeapons(bool includeNone = true)
  {
    var temp = new List<string>();
    if (includeNone) { temp.Add("[None]");}
    var path =  Application.dataPath + "/resources/Prefabs/Items/Weapons/";
    foreach (string file in System.IO.Directory.GetFiles(path))
    {
      string name = file.Substring(file.LastIndexOf('/')+1);
      if (name.Substring(name.Length - 4, 4) == "meta") { continue; }
      name = name.Substring(0,name.Length-7);
      temp.Add(name);
    }
    return temp.ToArray();

  }
  #endif
}