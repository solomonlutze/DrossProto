using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class DashAttributeTier : AttributeTier
{
  public float dashStaminaCostPercent;
  public bool ignoreFallsWhileDashing = false;
  public bool ignoreDamageWhileDashing = false;
  // public bool canFlyUp;
}
public class DashAttributeData : BaseAttributeData<DashAttributeTier>
{

  public float GetDashStaminaCostPercentForTier(int tier)
  {
    return GetAttributeTier(tier).dashStaminaCostPercent;
  }
  public bool GetDashingPreventsDamageForTier(int tier)
  {
    return GetAttributeTier(tier).ignoreDamageWhileDashing;
  }
  public bool GetDashingPreventsFallingForTier(int tier)
  {
    return GetAttributeTier(tier).ignoreFallsWhileDashing;
  }
  public float GetDashStaminaCost(Character c)
  {
    return GetDashStaminaCostPercentForTier(c.GetAttribute(CharacterAttribute.Dash)) / 100 * c.GetMaxStamina();
  }

  public bool GetDashingPreventsDamage(Character c)
  {
    return GetDashingPreventsDamageForTier(c.GetAttribute(CharacterAttribute.Dash));
  }
  public bool GetDashingPreventsFalling(Character c)
  {
    return GetDashingPreventsFallingForTier(c.GetAttribute(CharacterAttribute.Dash));
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