using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Should have a reference to AttributeValuePip prefab
// Should have references to its AttributeName, Description, and PipsContent children
// Should be passed an Attribute, its Value, and the corresponding AttributeData object
// Populate AttributeName and Description according to ATtributeData
// Init 1 pip for each point of Attribute
[ExecuteAlways]
public class AttributeInfo : MonoBehaviour
{

    public CharacterAttribute attribute;
    public GameObject attributeValuePipPrefab;
    public SuperTextMesh attributeNameText;
    public SuperTextMesh attributeDescriptionText;
    public SuperTextMesh pupaAttributeDescriptionText;
    public Transform pipsContainer;
    void Start()
    {

    }

    void Update()
    {
        if (!Application.IsPlaying(gameObject))
        {
            if (gameObject.name != attribute.ToString() + "_AttributeInfo")
            {
                gameObject.name = attribute.ToString() + "_AttributeInfo";
            }
        }
    }

    public void Init(AttributeData data, int value)
    {
        attributeNameText.text = data.displayName;
        attributeDescriptionText.text = data.attributeTiers[value].attributeTierDescription;

        // InitPips(value);
        DisableAllPips();
    }

    public void InitPips(int numberOfPips)
    {
        while (pipsContainer.childCount < numberOfPips)
        {
            Instantiate(attributeValuePipPrefab).transform.parent = pipsContainer;
        }
        for (int i = 0; i < pipsContainer.childCount; i++)
        {
            if (i < numberOfPips)
            {
                pipsContainer.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                pipsContainer.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    void DisableAllPips()
    {
        for (int i = 0; i < pipsContainer.childCount; i++)
        {
            pipsContainer.GetChild(i).gameObject.SetActive(false);
        }
    }
}
