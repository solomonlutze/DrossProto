using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisabledEquippedTraitButton : EquippedTraitButton {
	public override void Init(UpcomingLifeTrait upcomingTrait, InventoryScreen parentScreen, TraitSlot slot) {
    base.Init(upcomingTrait, parentScreen, slot);
		nameLabel.text = (upcomingTrait == null || upcomingTrait.trait == null) ? "No trait assigned"
			: upcomingTrait.trait.traitName + "\n(" +
        upcomingTrait.inventoryItem == null ?
          "(no item)"
          : upcomingTrait.inventoryItem.itemName + ")";
	}

	protected override void HandleClick() {
		return;
	}
}
