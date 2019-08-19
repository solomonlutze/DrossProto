using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquippedConsumableSlotButton : MonoBehaviour {

	public Button button;
	public SuperTextMesh nameLabel;
	private InventoryScreen inventoryScreen;
    private int slotNumber;

	public Color defaultColor;
	void Start () {
        button.onClick.AddListener(HandleClick);
		GetComponent<Image>().color = defaultColor;
	}

	public void Init(InventoryEntry equippedItemInfo, InventoryScreen parentScreen, int slot) {
        slotNumber = slot;
        nameLabel.text = equippedItemInfo == null ? "(Equip Consumable)" : equippedItemInfo.itemName;
		inventoryScreen = parentScreen;
	}

	private void HandleClick() {
		inventoryScreen.OpenEquipConsumableMenu(slotNumber);
	}
}
