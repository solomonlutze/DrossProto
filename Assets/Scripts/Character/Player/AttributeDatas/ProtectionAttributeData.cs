using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ProtectionAttribute
{
  Physical = CharacterAttribute.Resist_Physical,
  Fungal = CharacterAttribute.Resist_Fungal,
  Heat = CharacterAttribute.Resist_Heat,
  Acid = CharacterAttribute.Resist_Acid,
}

[System.Serializable]
public class ProtectionAttributeTier : AttributeTier
{
  public int protectionPercent;
}
public class ProtectionAttributeData : BaseAttributeData<ProtectionAttributeTier>
{
  public ProtectionAttribute protectionType;
  public static Dictionary<DamageType, ProtectionAttribute> DamageTypeToProtectionAttribute = new Dictionary<DamageType, ProtectionAttribute> {
    {DamageType.Physical, ProtectionAttribute.Physical },
    {DamageType.Fungal, ProtectionAttribute.Fungal },
    {DamageType.Heat, ProtectionAttribute.Heat },
    {DamageType.Acid, ProtectionAttribute.Acid },
  };

  public int GetProtectionPercentForTier(int tier)
  {
    Debug.Log("tier: " + tier + ", protection %: " + GetAttributeTier(tier).protectionPercent);
    return GetAttributeTier(tier).protectionPercent;
  }

  public int GetProtectionLevel(Character c)
  {
    return c.GetAttribute((CharacterAttribute)protectionType);
  }

  public int GetDamageReductionPercent(Character c)
  {
    Debug.Log("protection type: " + protectionType + "(int value: " + (int)protectionType + ")");
    return GetProtectionPercentForTier(c.GetAttribute((CharacterAttribute)protectionType));
  }

#if UNITY_EDITOR
  [MenuItem("Assets/Create/Attributes/ProtectionAttributeData")]
  public static void CreateProtectionAttributeData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Attribute Data", "New Protection Attribute Data", "Asset", "Save Attribute Data", "Assets/resources/Data/TraitData/Attributes");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ProtectionAttributeData>(), path);
  }
#endif
}