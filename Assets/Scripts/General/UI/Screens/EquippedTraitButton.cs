using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquippedTraitButton : MonoBehaviour {
	public Button button;
	public SuperTextMesh nameLabel;
	private InventoryScreen inventoryScreen;
    private int slotNumber;

	public Color defaultColor;
	protected UpcomingLifeTraits upcomingLifeTraits;
	protected TraitType traitType;
	// Use this for initialization
	void Start () {
		if (button != null) {
        	button.onClick.AddListener(HandleClick);
		}
		GetComponent<Image>().color = defaultColor;
	}
	
	public virtual void Init(UpcomingLifeTrait trait, InventoryScreen parentScreen, int slot, UpcomingLifeTraits upcomingLife, TraitType type) {
        slotNumber = slot;
        nameLabel.text = (trait == null) ? "(Assign " + type + " Trait)" 
			: trait.traitName + "\n(" + trait.inventoryItem.itemName + ")";
		this.inventoryScreen = parentScreen;
		this.upcomingLifeTraits = upcomingLife;
		this.traitType = type;
	}

	protected virtual void HandleClick() {
		inventoryScreen.OpenEquipTraitMenu(slotNumber, upcomingLifeTraits, traitType);
	}
}
