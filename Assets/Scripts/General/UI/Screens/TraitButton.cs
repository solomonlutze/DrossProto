using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// to equip a trait we need to know:
// which UpcomingLifeTraits we're equipping to
// which item we're representing
// which trait on the item - active or passive - we should show
public class TraitButton : ItemButton {

    private UpcomingLifeTraits lifeTraitsToEquipTo;
    private TraitType traitType;


    public void Init(InventoryEntry itemEntryInfo, InventoryScreen parentScreen, int? slot, UpcomingLifeTraits lifeTraits, TraitType type) {
        base.Init(itemEntryInfo, parentScreen, slot);
        lifeTraitsToEquipTo = lifeTraits;
		nameLabel.text = itemEntryInfo.itemName;
        traitType = type;
    }

    protected override void HandleClick() {
        Debug.Log("the correct handleclick");
		inventoryScreen.EquipTraitItem(item, slotNumber.GetValueOrDefault(), lifeTraitsToEquipTo, traitType);
	}
}