using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;


// Should be populated with a set of attributes
// Has a reference to AttributeInfo prefab and AttributesInfoContents
// For each attribute, creates and inits an AttributesInfo, and parents it to AttributesInfoContent
public class BugStatusView : MonoBehaviour
{

  public TraitSlotToBugTraitInfoDictionary traitInfos;
  public TraitSlotToBugTraitInfoDictionary nextTraitInfos;
  public SkillInfo[] skillInfoGameObjects;
  Dictionary<TraitSlot, TraitButton> traitButtons;
  Dictionary<CharacterAttribute, IAttributeDataInterface> attributeDataObjects;

  TraitSlotToTraitDictionary nextTraits;

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
  }

  public void Init(
    TraitSlotToTraitDictionary currentTraits,
    TraitPickupItem pickupItem
    )
  {
    displayedPickupItem = pickupItem;
    foreach (TraitSlot slot in currentTraits.Keys)
    {
      traitInfos[slot].Init(this, currentTraits[slot], slot);
    }

    foreach (TraitSlot slot in nextTraitInfos.Keys)
    {
      if (pickupItem == null)
      {
        nextTraits = new TraitSlotToTraitDictionary();
        nextTraitInfos[slot].gameObject.SetActive(false);
      }
      else
      {
        nextTraits = pickupItem.traits;
        nextTraitInfos[slot].Init(this, nextTraits[slot], slot);
        nextTraitInfos[slot].gameObject.SetActive(true);
      }

    }
  }

  // void InitSkillData(CharacterSkillData add = null, CharacterSkillData remove = null)
  // {
  //   CharacterSkillData skillData = null;
  //   CharacterSkillData pupaSkillData = null;
  //   for (int i = 0; i < skillInfoGameObjects.Length; i++)
  //   {
  //     skillData = i < skillDatas.Count ? skillDatas[i] : null;
  //     pupaSkillData = i < pupaSkillDatas.Count ? pupaSkillDatas[i] : null;
  //     if (pupaSkillData != null && pupaSkillData == remove && add != remove)
  //     {
  //       skillInfoGameObjects[i].Init(skillData, pupaSkillData, -1);
  //     }
  //     else if (i == pupaSkillDatas.Count && add != null)
  //     {
  //       skillInfoGameObjects[i].Init(skillData, add, 1);
  //     }
  //     else
  //     {
  //       skillInfoGameObjects[i].Init(skillData, pupaSkillData, 0);
  //     }
  //   }
  // }

  public void OnTraitButtonClicked(TraitSlot slot)
  {
    Debug.Log("equipping " + nextTraits[slot].skill.displayName + " to " + slot.ToString());
    GameMaster.Instance.GetPlayerController().EquipTrait(nextTraits[slot], slot);
    Destroy(displayedPickupItem.gameObject);
    GameMaster.Instance.canvasHandler.CloseMenus();
  }

}
