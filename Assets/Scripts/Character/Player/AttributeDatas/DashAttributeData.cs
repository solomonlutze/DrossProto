using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class DashAttributeTier : AttributeTier
{
  public int flightDuration;
  public bool canFlyUp;
}
public class DashAttributeData : BaseAttributeData<DashAttributeTier>
{

  public int GetFlightDurationForTier(int tier)
  {
    return GetAttributeTier(tier).flightDuration;
  }



#if UNITY_EDITOR
  [MenuItem("Assets/Create/Attributes/FlightAttributeData")]
  public static void CreateFlightAttributeData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Attribute Data", "New Flight Attribute Data", "Asset", "Save Attribute Data", "Assets/resources/Data/TraitData/Attributes");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<DashAttributeData>(), path);
  }
#endif
}