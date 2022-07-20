using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


// Should be populated with a set of attributes
// Has a reference to AttributeInfo prefab and AttributesInfoContents
// For each attribute, creates and inits an AttributesInfo, and parents it to AttributesInfoContent
public class BugStatusView : MenuBase
{

  public TraitSlotToBugTraitInfoDictionary traitInfos;
  public TraitSlotToBugTraitInfoDictionary nextTraitInfos;
  public SkillInfo[] skillInfoGameObjects;
  public UIBug uiBug;
  Dictionary<TraitSlot, TraitButton> traitButtons;
  Dictionary<CharacterAttribute, IAttributeDataInterface> attributeDataObjects;

  TraitSlotToTraitDictionary nextTraits;

  private PickupItem displayedPickupItem;

  private Trait traitToRemove;
  private Trait traitToAdd;

  public void Awake()
  {
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
      traitInfos[slot].Init(currentTraits[slot], slot);
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
        nextTraitInfos[slot].Init(nextTraits[slot], slot);
        nextTraitInfos[slot].gameObject.SetActive(true);
      }
    }
    uiBug.Init(currentTraits);
  }

  public void OnTraitButtonClicked(TraitSlot slot)
  {
    GameMaster.Instance.GetPlayerController().EquipTrait(nextTraits[slot], slot);
    Destroy(displayedPickupItem.gameObject);
    GameMaster.Instance.canvasHandler.CloseMenus();
  }

  public void OnCloseButtonClicked()
  {
    GameMaster.Instance.canvasHandler.CloseMenus();
  }

  public void OnTraitButtonSelected(TraitSlot slot)
  {
    uiBug.HighlightSlot(slot, nextTraits[slot]);
  }

  public void OnTraitButtonDeselected()
  {
    uiBug.UnhighlightSlot();
  }
}
