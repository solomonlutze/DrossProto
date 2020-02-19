using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class AttributeTier
{
    public string attributeTierDescription;
    [TextArea]
    public string attributeTierDescriptionLong;
}
public class AttributeData : ScriptableObject
{
    public CharacterAttribute attribute;
    public string displayName;
    public AttributeTier[] attributeTiers;
    // Hide this attribute in the AttributesView screen if its value is zero
    public bool hideIfZero = false;


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
