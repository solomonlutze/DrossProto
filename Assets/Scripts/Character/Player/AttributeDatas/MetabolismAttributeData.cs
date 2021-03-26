using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class MetabolismAttributeTier : AttributeTier
{
  [Tooltip("Time it takes to recover 100% of stamina, in seconds")]
  public float staminaRecoverySpeed;
  // public bool canFlyUp;
}
public class MetabolismAttributeData : BaseAttributeData<MetabolismAttributeTier>
{

  public float GetStaminaRecoverySpeedForTier(int tier)
  {
    return GetAttributeTier(tier).staminaRecoverySpeed;
  }

  public float GetStaminaRecoverySpeed(Character c)
  {
    return GetStaminaRecoverySpeedForTier(c.GetAttribute(CharacterAttribute.Metabolism));
  }


#if UNITY_EDITOR
  [MenuItem("Assets/Create/Attributes/MetabolismAttributeData")]
  public static void CreateMetabolismAttributeData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Attribute Data", "New Metabolism Attribute Data", "Asset", "Save Attribute Data", "Assets/resources/Data/TraitData/Attributes");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<MetabolismAttributeData>(), path);
  }
#endif
}