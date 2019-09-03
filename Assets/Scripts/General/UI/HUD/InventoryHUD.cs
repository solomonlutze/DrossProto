using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryHUD : MonoBehaviour {

	private SuperTextMesh drossFieldText;
	private SuperTextMesh drossValueText;
    private SuperTextMesh itemCollectedText1;
    private SuperTextMesh itemCollectedText2;
    private SuperTextMesh itemCollectedText3;
    private SuperTextMesh usingItemText;

	// Use this for initialization
	// TODO: wtf these should be public fields we set in the inspector, this is NONSENSE
	void Start () {
		drossFieldText = transform.Find("DrossFieldText").GetComponent<SuperTextMesh>();
		drossValueText = transform.Find("DrossValueText").GetComponent<SuperTextMesh>();
		itemCollectedText1 = transform.Find("ItemCollectedText1").GetComponent<SuperTextMesh>();
		itemCollectedText2 = transform.Find("ItemCollectedText2").GetComponent<SuperTextMesh>();
		itemCollectedText3 = transform.Find("ItemCollectedText3").GetComponent<SuperTextMesh>();
		usingItemText = transform.Find("UsingItemText").GetComponent<SuperTextMesh>();

	}

	// Update is called once per frame
	void Update () {
		PlayerController playerController = GameMaster.Instance.GetPlayerController();
		if (playerController != null) {
            Inventory inventory = playerController.GetComponent<Inventory>();
			if (inventory != null) {

				InventoryEntry dross = inventory.GetInventoryEntry("Dross");
				drossFieldText.text = "Dross: ";
				drossValueText.text = dross != null ? dross.quantity+"" : "0";
				itemCollectedText1.text = inventory.lastPickedUpItems.Count > 0 ?
					FormatPickedUpItemText(inventory.lastPickedUpItems[0]) : " ";
				itemCollectedText2.text = inventory.lastPickedUpItems.Count > 1 ?
					FormatPickedUpItemText(inventory.lastPickedUpItems[1]) : " ";
				itemCollectedText3.text = inventory.lastPickedUpItems.Count > 2 ?
					FormatPickedUpItemText(inventory.lastPickedUpItems[2]) : " ";
				usingItemText.text = FormatLastUsedItemText(inventory.lastUsedItem);
			}
		}
	}

	string FormatLastUsedItemText(string itemName) {
		return itemName != null && itemName.Length > 0 ? string.Format("Used {0}!", itemName) : " ";
	}

	string FormatPickedUpItemText(PickedUpItem invItem) {
		string ret = invItem.itemName;
		if (invItem.quantity > 1) {
			ret += " ("+invItem.quantity+")";
		}
		return ret;
	}
}
