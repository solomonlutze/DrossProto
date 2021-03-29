using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SightRangeAttributeTier : AttributeTier
{
  [Tooltip("Number of tiles away the player can see. Absolute range of NPC vision.")]
  public int sightRange;
}
public class SightRangeAttributeData : BaseAttributeData<SightRangeAttributeTier>
{

  public int GetSightRangeForTier(int tier)
  {
    return GetAttributeTier(tier).sightRange;
  }

  public int GetSightRange(Character c)
  {
    return GetSightRangeForTier(c.GetAttribute(CharacterAttribute.SightRange));
  }


#if UNITY_EDITOR
  [MenuItem("Assets/Create/Attributes/SightRangeAttributeData")]
  public static void CreateSightRangeAttributeData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Attribute Data", "New SightRange Attribute Data", "Asset", "Save Attribute Data", "Assets/resources/Data/TraitData/Attributes");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<SightRangeAttributeData>(), path);
  }
#endif
}