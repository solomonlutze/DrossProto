using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

[System.Serializable]
public class BugPresetData : ScriptableObject
{

  public string displayName;
  [TextArea]
  public string description;
  public TraitSlotToTraitDictionary loadout;

#if UNITY_EDITOR
  // The following is a helper that adds a menu item to create an TraitItem Asset
  [MenuItem("Assets/Create/Presets/BugPreset")]
  public static void CreateBugPreset()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Bug Preset", "New Bug Preset", "Asset", "Save Bug Preset", "Assets/resources/Data/CharacterData/PresetData");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<BugPresetData>(), path);
  }
#endif
}