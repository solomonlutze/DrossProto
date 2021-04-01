using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class MoltingEfficiencyAttributeTier : AttributeTier
{
  public float moltCarapaceCostPercent;
  // public bool canFlyUp;
}
public class MoltingEfficiencyAttributeData : BaseAttributeData<MoltingEfficiencyAttributeTier>
{

  public float GetMoltCarapaceCostPercentForTier(int tier)
  {
    return GetAttributeTier(tier).moltCarapaceCostPercent;
  }

  public float GetMoltCarapaceCost(Character c)
  {
    return GetMoltCarapaceCostPercentForTier(c.GetAttribute(CharacterAttribute.MoltingEfficiency)) / 100 * c.GetMaxCarapace();
  }


#if UNITY_EDITOR
  [MenuItem("Assets/Create/Attributes/MoltingEfficiencyAttributeData")]
  public static void CreateMoltingEfficiencyAttributeData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Attribute Data", "New MoltingEfficiency Attribute Data", "Asset", "Save Attribute Data", "Assets/resources/Data/TraitData/Attributes");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<MoltingEfficiencyAttributeData>(), path);
  }
#endif
}