using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class FlightAttributeTier : AttributeTier
{
  public int flightDuration;
  public bool canFly;
  public bool canFlyUp;
}
public class FlightAttributeData : BaseAttributeData<FlightAttributeTier>
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
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<FlightAttributeData>(), path);
  }
#endif
}