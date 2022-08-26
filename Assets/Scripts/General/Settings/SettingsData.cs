using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
[System.Serializable]
public class SettingsData : ScriptableObject
{

  public List<TraitSlot> bodyPartBreakOrder = new List<TraitSlot>(){
    TraitSlot.Wings,
    TraitSlot.Abdomen,
    TraitSlot.Legs,
    TraitSlot.Head,
    };
#if UNITY_EDITOR
  [MenuItem("Assets/Create/SettingsData")]
  public static void CreateSettingsData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Settings Data", "New Settings Data", "Asset", "Save Settings Data", "Assets/resources/Data/General");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<SettingsData>(), path);
  }
#endif
}
