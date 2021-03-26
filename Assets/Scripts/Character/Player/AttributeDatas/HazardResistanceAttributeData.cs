using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class HazardResistanceAttributeTier : AttributeTier
{
  public float HazardImmunityDurationMultiplier;
  // public bool canFlyUp;
}
public class HazardResistanceAttributeData : BaseAttributeData<HazardResistanceAttributeTier>
{

  public float GetHazardImmunityDurationMultiplierForTier(int tier)
  {
    return GetAttributeTier(tier).HazardImmunityDurationMultiplier;
  }

  public float GetHazardImmunityDurationMultiplier(Character c)
  {
    return GetHazardImmunityDurationMultiplierForTier(c.GetAttribute(CharacterAttribute.HazardResistance));
  }


#if UNITY_EDITOR
  [MenuItem("Assets/Create/Attributes/HazardResistanceAttributeData")]
  public static void CreateHazardResistanceAttributeData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Attribute Data", "New Hazard Resist Attribute Data", "Asset", "Save Attribute Data", "Assets/resources/Data/TraitData/Attributes");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<HazardResistanceAttributeData>(), path);
  }
#endif
}