using System;
using System.Linq;
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

    private Trait traitToRemove;
    private Trait traitToAdd;

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
            GameObject button = Instantiate(traitButtonPrefab, traitButtonsContainer);
            // button.transform.parent = traitButtonsContainer;
            traitButtons.Add(slot, button.GetComponentInChildren<TraitButton>());
        }
    }

    public void Init(CharacterAttributeToIntDictionary attributes, CharacterAttributeToIntDictionary nextAttributes, TraitSlotToTraitDictionary pupaTraits, TraitPickupItem traitPickupItem = null)
    {
        foreach (CharacterAttribute attribute in attributes.Keys)
        {
            AddOrUpdateAttributeInfoObject(attribute, attributes[attribute], nextAttributes[attribute]);
        }
        displayedPickupItem = traitPickupItem;
        if (pupaTraits != null)
        {
            Trait itemTrait;
            foreach (TraitSlot slot in pupaTraits.Keys)
            {
                itemTrait = null;
                if (traitPickupItem != null && traitPickupItem.traits.ContainsKey(slot)) { itemTrait = traitPickupItem.traits[slot]; }
                traitButtons[slot].gameObject.SetActive(true);
                traitButtons[slot].Init(slot, pupaTraits[slot], itemTrait, this, attributeDataObjects);
            }
        }
        else
        {
            // foreach (TraitButton button in traitButtons.Values)
            // {
            //     button.gameObject.SetActive(false);
            // }
        }
    }

    public void AddOrUpdateAttributeInfoObject(CharacterAttribute attribute, int value, int nextValue)
    {
        if (!attributeInfoGameObjects.ContainsKey(attribute))
        {
            attributeInfoGameObjects.Add(attribute, Instantiate(attributeInfoPrefab).gameObject);
            attributeInfoGameObjects[attribute].transform.parent = attributeInfosContainer;
        }

        if (!attributeDataObjects.ContainsKey(attribute))
        {
            return;
        }

        GameObject go = attributeInfoGameObjects[attribute];

        go.GetComponent<AttributeInfo>().Init(attributeDataObjects[attribute], value, nextValue);
    }

    public void OnTraitButtonClicked(Trait trait, TraitSlot slot)
    {
        GameMaster.Instance.GetPlayerController().EquipTrait(trait, slot);
        Destroy(displayedPickupItem.gameObject);
        GameMaster.Instance.canvasHandler.SetAllCanvasesInactive();
    }

    public void ShowHighlightedTraitDelta(Trait remove, Trait add)
    {
        if (add != null && remove != null)
        {
            traitToRemove = remove;
            traitToAdd = add;
            int proposedAttributeAdd = 0;
            int proposedAttributeRemove = 0;
            foreach (CharacterAttribute attribute in remove.attributeModifiers.Keys.Union(add.attributeModifiers.Keys))
            {
                proposedAttributeAdd = 0;
                proposedAttributeRemove = 0;
                if (traitToAdd != null && traitToRemove != null)
                {
                    traitToAdd.attributeModifiers.TryGetValue(attribute, out proposedAttributeAdd);
                    traitToRemove.attributeModifiers.TryGetValue(attribute, out proposedAttributeRemove);
                }
                attributeInfoGameObjects[attribute].GetComponent<AttributeInfo>().HighlightDelta(proposedAttributeAdd - proposedAttributeRemove);
            }
        }
    }

    public void UnshowHighlightedTraitDelta()
    {

        foreach (CharacterAttribute attribute in traitToRemove.attributeModifiers.Keys.Union(traitToAdd.attributeModifiers.Keys))
        {
            attributeInfoGameObjects[attribute].GetComponent<AttributeInfo>().UnhighlightDelta();
        }
        traitToRemove = null;
        traitToAdd = null;
    }
}
