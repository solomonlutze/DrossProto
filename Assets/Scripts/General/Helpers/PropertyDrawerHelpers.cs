
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public static class PropertyDrawerHelpers {
  #if UNITY_EDITOR


  public static string[] AllActiveTraitNames(bool includeNone = false)
  {
    var temp = new List<string>();
    if (includeNone) { temp.Add("[None]");}
    var path =  Application.dataPath + "/resources/Data/TraitData/ActiveTraits/";
    foreach (string file in System.IO.Directory.GetFiles(path))
    {
      string name = file.Substring(file.LastIndexOf('/')+1);
      if (name.Substring(name.Length - 4, 4) == "meta") { continue; }
      name = name.Substring(0,name.Length-6);
      temp.Add(name);
    }
    return temp.ToArray();
  }

  public static string[] AllPassiveTraitNames(bool includeNone = false)
  {
    var temp = new List<string>();
    if (includeNone) { temp.Add("[None]");}
    var path =  Application.dataPath + "/resources/Data/TraitData/PassiveTraits/";
    foreach (string file in System.IO.Directory.GetFiles(path))
    {
      string name = file.Substring(file.LastIndexOf('/')+1);
      if (name.Substring(name.Length - 4, 4) == "meta") { continue; }
      name = name.Substring(0,name.Length-3);
      temp.Add(name);
    }
    return temp.ToArray();
  }

  public static string[] AllAttackAnimationNames(bool includeNone = false)
  {
    var temp = new List<string>();
    if (includeNone) { temp.Add("[None]");}
    var path =  Application.dataPath + "/resources/Data/AnimationData/Attack/";
    foreach (string file in System.IO.Directory.GetFiles(path))
    {
      string name = file.Substring(file.LastIndexOf('/')+1);
      if (name.Substring(name.Length - 4, 4) == "meta") { continue; }
      if (name.Substring(name.Length - 5, 5) != "Asset") { continue; }
      name = name.Substring(0,name.Length-6);
      temp.Add(name);
    }
    return temp.ToArray();
  }

  public static string[] AllItemsOfType(string itemType)
  {
    var temp = new List<string>();
    var path =  Application.dataPath + "/resources/Data/ItemData/"+itemType+"/";
    foreach (string file in System.IO.Directory.GetFiles(path))
    {
      string name = file.Substring(file.LastIndexOf('/')+1);
      if (name.Substring(name.Length - 4, 4) == "meta") { continue; }
      name = name.Substring(0,name.Length-5);
      temp.Add(name);
    }
    return temp.ToArray();

  }
  #endif
}