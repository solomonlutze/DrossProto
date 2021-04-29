using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class DarkVisionInfo
{
  public float minIllumination;
  public float visibilityMultiplier;
}
[System.Serializable]
public class DarkVisionAttributeTier : AttributeTier
{
  public DarkVisionInfo[] darkVisionInfos;
}
public class DarkVisionAttributeData : BaseAttributeData<DarkVisionAttributeTier>
{

  public DarkVisionInfo[] GetDarkVisionInfosForTier(int tier)
  {
    return GetAttributeTier(tier).darkVisionInfos;
  }

  public DarkVisionInfo[] GetDarkVisionInfos(Character c)
  {
    return GetDarkVisionInfosForTier(c.GetAttribute(CharacterAttribute.DarkVision));
  }

  public static float GetVisibilityMultiplierForTile(DarkVisionInfo[] infos, EnvironmentTileInfo t)
  {
    return 1;
    for (int i = 0; i < infos.Length; i++)
    {
      if (t.illuminationInfo.illuminationLevel >= infos[i].minIllumination)
      {
        return infos[i].visibilityMultiplier;
      }
    }
    return 1;
  }

#if UNITY_EDITOR
  [MenuItem("Assets/Create/Attributes/DarkVisionAttributeData")]
  public static void CreateDarkVisionAttributeData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Attribute Data", "New Dark Vision Attribute Data", "Asset", "Save Attribute Data", "Assets/resources/Data/TraitData/Attributes");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<DarkVisionAttributeData>(), path);
  }
#endif
}