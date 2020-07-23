using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class HealthAttributeTier : AttributeTier
{
  public int maxHealth;
}
public class HealthAttributeData : BaseAttributeData<HealthAttributeTier>
{

  public int GetMaxHealthForTier(int tier)
  {
    return GetAttributeTier(tier).maxHealth;
  }

  public int GetMaxHealth(Character c)
  {
    return GetMaxHealthForTier(c.GetAttribute(CharacterAttribute.Health));
  }


#if UNITY_EDITOR
  [MenuItem("Assets/Create/Attributes/HealthAttributeData")]
  public static void CreateHealthAttributeData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Attribute Data", "New Health Attribute Data", "Asset", "Save Attribute Data", "Assets/resources/Data/TraitData/Attributes");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<HealthAttributeData>(), path);
  }
#endif
}