using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class CamouflageAttributeTier : AttributeTier
{
  [Tooltip("Range at which character is invisible, in units. Negative indicates that the character is always visible.")]
  public float camouflageDistance;
}
public class CamouflageAttributeData : BaseAttributeData<CamouflageAttributeTier>
{

  public float GetCamouflageDistanceForTier(int tier)
  {
    return GetAttributeTier(tier).camouflageDistance;
  }

  public float GetCamouflageDistance(Character c)
  {
    return GetCamouflageDistanceForTier(c.GetAttribute(CharacterAttribute.Camouflage));
  }


#if UNITY_EDITOR
  [MenuItem("Assets/Create/Attributes/CamouflageAttributeData")]
  public static void CreateCamouflageAttributeData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Attribute Data", "Camouflage", "Asset", "Save Attribute Data", "Assets/resources/Data/TraitData/Attributes");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<CamouflageAttributeData>(), path);
  }
#endif
}