using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public interface IAttributeDataInterface
{
  AttributeTier[] attributeTiers { get; }
  CharacterAttribute attribute { get; }
  string displayName { get; }
  string displayNameWithGroup { get; }
  // Hide this attribute in the AttributesView screen if its value is zero
  bool hideIfZero { get; }
  bool ignoreInMenus { get; }
}

public abstract class BaseAttributeData<T> : ScriptableObject, IAttributeDataInterface where T : AttributeTier
{
  [SerializeField]
  protected T[] _attributeTiers;
  public AttributeTier[] attributeTiers { get { return _attributeTiers; } }

  [SerializeField]
  protected CharacterAttribute _attribute;
  public CharacterAttribute attribute { get { return _attribute; } }
  [SerializeField]
  protected string _displayName;
  public string displayName { get { return _displayName; } }
  [SerializeField]
  protected string _displayNameWithGroup;
  public string displayNameWithGroup { get { return _displayNameWithGroup; } }
  [SerializeField]
  protected bool _hideIfZero;
  public bool hideIfZero { get { return _hideIfZero; } }

  [SerializeField]
  protected bool _ignoreInMenus;
  public bool ignoreInMenus { get { return _ignoreInMenus; } }


  public T GetAttributeTier(Character c)
  {
    return attributeTiers[c.GetAttribute(attribute)] as T;
  }
  public T GetAttributeTier(int tier)
  {
    return attributeTiers[tier] as T;
  }
}

// public class BaseAttribute : MyBaseClass<AttributeTier>
// {
// #if UNITY_EDITOR
//   // The following is a helper that adds a menu item to create a AttributeData Asset
//   [MenuItem("Assets/Create/Attributes/AttributeData")]
//   public static void CreateAttributeData()
//   {
//     string path = EditorUtility.SaveFilePanelInProject("Save Attribute Data", "New Attribute Data", "Asset", "Save Attribute Data", "Assets/resources/Data/TraitData/Attributes");
//     if (path == "")
//       return;
//     AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<BaseAttribute>(), path);
//   }
// #endif
// }

// public class MySubClass : MyBaseClass<FlightAttributeTier>
// {
// #if UNITY_EDITOR
//   [MenuItem("Assets/Create/Attributes/FlightAttributeData")]
//   public static void CreateFlightAttributeData()
//   {
//     string path = EditorUtility.SaveFilePanelInProject("Save Attribute Data", "New Flight Attribute Data", "Asset", "Save Attribute Data", "Assets/resources/Data/TraitData/Attributes");
//     if (path == "")
//       return;
//     AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<MySubClass>(), path);
//   }
// #endif
// }

[System.Serializable]
public class AttributeTier
{
  public string attributeTierDescription;
  [TextArea]
  public string attributeTierDescriptionLong;
}
public class AttributeData : BaseAttributeData<AttributeTier>
{


#if UNITY_EDITOR
  // The following is a helper that adds a menu item to create a AttributeData Asset
  [MenuItem("Assets/Create/Attributes/AttributeData")]
  public static void CreateAttributeData()
  {
    string path = EditorUtility.SaveFilePanelInProject("Save Attribute Data", "New Attribute Data", "Asset", "Save Attribute Data", "Assets/resources/Data/TraitData/Attributes");
    if (path == "")
      return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<AttributeData>(), path);
  }
#endif

}
