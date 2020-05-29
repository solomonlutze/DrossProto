using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class DashAttributeTier : AttributeTier
{
  public float dashStaminaCostPercent;
  // public bool canFlyUp;
}
public class DashAttributeData : BaseAttributeData<DashAttributeTier>
{

  public float GetDashStaminaCostPercentForTier(int tier)
  {
    return GetAttributeTier(tier).dashStaminaCostPercent;
  }

  public float GetDashStaminaCost(Character c)
  {
    return GetDashStaminaCostPercentForTier(c.GetAttribute(CharacterAttribute.Dash)) / 100 * c.GetMaxStamina();
  }


#if UNITY_EDITOR
  [MenuItem("Assets/Create/Attributes/DashAttributeData")]
  public static void CreateDashAttributeData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Attribute Data", "New Dash Attribute Data", "Asset", "Save Attribute Data", "Assets/resources/Data/TraitData/Attributes");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<DashAttributeData>(), path);
  }
#endif
}