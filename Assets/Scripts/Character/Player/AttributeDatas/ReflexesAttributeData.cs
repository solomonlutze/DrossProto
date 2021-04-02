using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ReflexesAttributeTier : AttributeTier
{
  public float moveSpeedMultiplier;
}
public class ReflexesAttributeData : BaseAttributeData<ReflexesAttributeTier>
{

  public float GetMoveSpeedMultiplierForTier(int tier)
  {
    return GetAttributeTier(tier).moveSpeedMultiplier;
  }

  public float GetMoveSpeedMultiplier(Character c)
  {
    return GetMoveSpeedMultiplierForTier(c.GetAttribute(CharacterAttribute.Reflexes));
  }


#if UNITY_EDITOR
  [MenuItem("Assets/Create/Attributes/ReflexesAttributeData")]
  public static void CreateReflexesAttributeData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Attribute Data", "New Reflexes Attribute Data", "Asset", "Save Attribute Data", "Assets/resources/Data/TraitData/Attributes");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ReflexesAttributeData>(), path);
  }
#endif
}