using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Should be populated with a set of attributes
// Has a reference to AttributeInfo prefab and AttributesInfoContents
// For each attribute, creates and inits an AttributesInfo, and parents it to AttributesInfoContent
public class AttributesView : MonoBehaviour
{

    public AttributeInfo attributeInfoPrefab;
    public Transform attributeInfosContainer;

    public CharacterAttributeToGameObjectDictionary attributeInfoGameObjects;
    // Dictionary<CharacterAttribute, GameObject> attributeInfoGameObjects;
    Dictionary<CharacterAttribute, AttributeData> attributeDataObjects;
    public void Awake()
    {
        // attributeInfoGameObjects = new Dictionary<CharacterAttribute, GameObject>();
        attributeDataObjects = new Dictionary<CharacterAttribute, AttributeData>();
        Object[] dataObjects = Resources.LoadAll("Data/TraitData/Attributes");
        foreach (Object obj in dataObjects)
        {
            AttributeData attrObj = obj as AttributeData;
            if (attrObj == null) { continue; }
            attributeDataObjects.Add(attrObj.attribute, attrObj);
        }
    }

    public void Init(CharacterAttributeToIntDictionary attributes)
    {
        foreach (CharacterAttribute attribute in attributes.Keys)
        {
            AddOrUpdateAttributeInfoObject(attribute, attributes[attribute]);
        }
    }

    public void AddOrUpdateAttributeInfoObject(CharacterAttribute attribute, int value)
    {
        if (!attributeInfoGameObjects.ContainsKey(attribute))
        {
            attributeInfoGameObjects.Add(attribute, Instantiate(attributeInfoPrefab).gameObject);
            attributeInfoGameObjects[attribute].transform.parent = attributeInfosContainer;
        }
        if (!attributeInfoGameObjects.ContainsKey(attribute))
        {
            Debug.LogError("Couldn't find attributeInfoGameOBject " + attribute);
            return;
        }

        if (!attributeDataObjects.ContainsKey(attribute))
        {
            Debug.LogError("Couldn't find attribute data for" + attribute);
            return;
        }

        GameObject go = attributeInfoGameObjects[attribute];
        go.GetComponent<AttributeInfo>().Init(attributeDataObjects[attribute], value);
    }

}
