using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisabledEquippedTraitButton : EquippedTraitButton {
	public override void Init(UpcomingLifeTrait trait, InventoryScreen parentScreen, int slot, UpcomingLifeTraits upcomingLife, TraitType type) {
        base.Init(trait, parentScreen, slot, upcomingLife, type);
		nameLabel.text = (trait == null) ? "No "+type+" trait assigned" 
			: trait.traitName + "\n(" + trait.inventoryItem.itemName + ")";
	}
	protected override void HandleClick() {
		return;
	}
}
