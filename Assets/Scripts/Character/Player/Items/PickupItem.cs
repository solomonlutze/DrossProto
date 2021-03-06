﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItem : Interactable {

	public int quantity;

	public InventoryItemType itemType;
	// public string itemId;

	[HideInInspector]
	public string itemId;
	public override string interactableText {
		get { return "Collect"; }
		set { }
	}

	void PlayerActivate () {
		if (GameMaster.Instance.GetPlayerController() != null) {
			GameMaster.Instance.GetPlayerController().AddToInventory(this);
			Destroy(gameObject);
		}
	}

}
