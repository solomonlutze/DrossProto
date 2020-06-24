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
  public SkillInfo[] skillInfoGameObjects;
  Dictionary<TraitSlot, TraitButton> traitButtons;
  Dictionary<CharacterAttribute, IAttributeDataInterface> attributeDataObjects;

  private PickupItem displayedPickupItem;

  private Trait traitToRemove;
  private Trait traitToAdd;

  public void Awake()
  {
    // attributeInfoGameObjects = new Dictionary<CharacterAttribute, GameObject>();
    attributeDataObjects = new Dictionary<CharacterAttribute, IAttributeDataInterface>();
    traitButtons = new Dictionary<TraitSlot, TraitButton>();
    UnityEngine.Object[] dataObjects = Resources.LoadAll("Data/TraitData/Attributes");
    foreach (UnityEngine.Object obj in dataObjects)
    {
      IAttributeDataInterface attrObj = obj as IAttributeDataInterface;
      if (attrObj == null) { continue; }
      attributeDataObjects.Add(attrObj.attribute, attrObj);
    }
    foreach (TraitSlot slot in Enum.GetValues(typeof(TraitSlot)))
    {
      GameObject button = Instantiate(traitButtonPrefab, traitButtonsContainer);
      traitButtons.Add(slot, button.GetComponentInChildren<TraitButton>());
    }
  }

  public void Init(
    CharacterAttributeToIntDictionary attributes,
    CharacterAttributeToIntDictionary nextAttributes,
    List<CharacterSkillData> skillDatas,
    List<CharacterSkillData> pupaSkillDatas,
    TraitSlotToTraitDictionary pupaTraits,
    TraitPickupItem traitPickupItem = null
    )
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
      CharacterSkillData skillData = null;
      CharacterSkillData pupaSkillData = null;
      for (int i = 0; i < skillInfoGameObjects.Length; i++)
      {
        skillData = i < skillDatas.Count ? skillDatas[i] : null;
        pupaSkillData = i < pupaSkillDatas.Count ? pupaSkillDatas[i] : null;
        if (skillData == null && pupaSkillData == null)
        {
          skillInfoGameObjects[i].gameObject.SetActive(false);
        }
        else
        {
          skillInfoGameObjects[i].gameObject.SetActive(true);
          skillInfoGameObjects[i].Init(skillData, pupaSkillData, 0);
        }
      }
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
    Debug.Log("adding: " + add + ", removing " + remove);
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
      if (remove.skillData != add.skillData)
      {

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
