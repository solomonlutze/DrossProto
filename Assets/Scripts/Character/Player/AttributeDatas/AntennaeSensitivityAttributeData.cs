using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class AntennaeSensitivityAttributeTier : AttributeTier
{
  public float awarenessRange;
}
public class AntennaeSensitivityAttributeData : BaseAttributeData<AntennaeSensitivityAttributeTier>
{

  public float GetAwarenessRangeForTier(int tier)
  {
    return GetAttributeTier(tier).awarenessRange;
  }

  public float GetAwarenessRange(Character c)
  {
    return GetAwarenessRangeForTier(c.GetAttribute(CharacterAttribute.AntennaeSensitivity));
  }


#if UNITY_EDITOR
  [MenuItem("Assets/Create/Attributes/AntennaeSensitivityAttributeData")]
  public static void CreateAntennaeSensitivityAttributeData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Attribute Data", "New AntennaeSensitivity Attribute Data", "Asset", "Save Attribute Data", "Assets/resources/Data/TraitData/Attributes");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AntennaeSensitivityAttributeData>(), path);
  }
#endif
}