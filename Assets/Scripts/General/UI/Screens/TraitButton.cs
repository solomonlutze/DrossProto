using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// to equip a trait we need to know:
// which UpcomingLifeTraits we're equipping to
// which item we're representing
// which trait on the item - active or passive - we should show
public class TraitButton : ItemButton {
    private TraitSlot traitSlot;


    public void Init(InventoryEntry itemEntryInfo, InventoryScreen parentScreen, TraitSlot ts) {
        base.Init(itemEntryInfo, parentScreen);
        traitSlot = ts;
		    nameLabel.text = itemEntryInfo.itemName;
    }

  protected override void HandleClick() {
		inventoryScreen.EquipTraitItem(item, traitSlot);
	}
}