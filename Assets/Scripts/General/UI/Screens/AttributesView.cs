using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Should be populated with a set of attributes
// Has a reference to AttributeInfo prefab and AttributesInfoContents
// For each attribute, creates and inits an AttributesInfo, and parents it to AttributesInfoContent
public class AttributesView : MonoBehaviour
{

    public AttributeInfo attributeInfoPrefab;

    public GameObject traitButtonPrefab;
    public Transform attributeInfosContainer;
    public Transform traitButtonsContainer;

    public CharacterAttributeToGameObjectDictionary attributeInfoGameObjects;
    Dictionary<TraitSlot, TraitButton> traitButtons;
    Dictionary<CharacterAttribute, AttributeData> attributeDataObjects;

    private PickupItem displayedPickupItem;

    public void Awake()
    {
        // attributeInfoGameObjects = new Dictionary<CharacterAttribute, GameObject>();
        attributeDataObjects = new Dictionary<CharacterAttribute, AttributeData>();
        traitButtons = new Dictionary<TraitSlot, TraitButton>();
        UnityEngine.Object[] dataObjects = Resources.LoadAll("Data/TraitData/Attributes");
        foreach (UnityEngine.Object obj in dataObjects)
        {
            AttributeData attrObj = obj as AttributeData;
            if (attrObj == null) { continue; }
            attributeDataObjects.Add(attrObj.attribute, attrObj);
        }
        foreach (TraitSlot slot in Enum.GetValues(typeof(TraitSlot)))
        {
            GameObject button = Instantiate(traitButtonPrefab);
            button.transform.parent = traitButtonsContainer;
            traitButtons.Add(slot, button.GetComponentInChildren<TraitButton>());
        }
    }

    public void Init(CharacterAttributeToIntDictionary attributes, TraitSlotToTraitDictionary currentTraits, TraitPickupItem traitPickupItem = null)
    {
        foreach (CharacterAttribute attribute in attributes.Keys)
        {
            AddOrUpdateAttributeInfoObject(attribute, attributes[attribute]);
        }
        displayedPickupItem = traitPickupItem;
        if (currentTraits != null && displayedPickupItem != null)
        {
            Trait itemTrait;
            foreach (TraitSlot slot in currentTraits.Keys)
            {
                itemTrait = null;
                if (traitPickupItem.traits.ContainsKey(slot)) { itemTrait = traitPickupItem.traits[slot]; }
                traitButtons[slot].Init(slot, currentTraits[slot], itemTrait, this);
            }
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
            return;
        }

        GameObject go = attributeInfoGameObjects[attribute];
        go.GetComponent<AttributeInfo>().Init(attributeDataObjects[attribute], value);
    }

    public void OnTraitButtonClicked(Trait trait, TraitSlot slot)
    {
        GameMaster.Instance.GetPlayerController().EquipTrait(trait, slot);
        Destroy(displayedPickupItem.gameObject);
        GameMaster.Instance.canvasHandler.SetAllCanvasesInactive();
    }
}
