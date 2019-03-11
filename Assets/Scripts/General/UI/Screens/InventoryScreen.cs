using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// When inventory is opened it should show equipment first:
// - current weapon
// - current quick items
// When a piece of equipment is clicked, it opens the relevant item list (consumables or weapons)
// Equipped items in the item list are a different color
// Clicking an item equips it 

public class InventoryScreen : MonoBehaviour {

	public Transform weaponsListContent;
	public GameObject itemButtonPrefab;
	public GameObject traitButtonPrefab;

	public Transform consumableList;
	public SuperTextMesh consumableListHeader;
	public Transform consumableListContent;

	public Transform equippedConsumableList;
	public Transform equippedConsumableListContent;
	public GameObject equippedConsumableButtonPrefab;

	public Transform equippedTraitListLarva;
	public Transform equippedTraitListLarvaContent;
	public Transform equippedTraitListPupa;
	public Transform equippedTraitListPupaContent;
	public GameObject equippedTraitButtonPrefab;
	public GameObject disabledEquippedTraitButtonPrefab;


	public Transform traitList;
	public Transform traitListContent;

	void Awake () {
		consumableList.gameObject.SetActive(false);
		traitList.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	public void Enable() {
		PlayerController playerController = GameMaster.Instance.GetPlayerController();
		if (playerController != null) {
			CloseEquipConsumableMenu();
			CloseEquipTraitMenu();
			
            Inventory inventory = playerController.GetComponent<Inventory>();
		 	PopulateEquippableItemList(inventory.GetAllItemsOfType(InventoryItemType.Weapon), weaponsListContent, null);
		 	PopulateEquippedConsumablesList(inventory.GetEquippedConsumableInventoryEntries());
			PopulateEquippedTraitsList(inventory.GetUpcomingLarva(), inventory.GetUpcomingPupa());
		}
	}

	public void PopulateEquippedConsumablesList(InventoryEntry[] equippedItems) {
		foreach(Transform child in equippedConsumableListContent) {
			Destroy(child.gameObject);
		}
		for (int i = 0; i < equippedItems.Length; i++) {
			InventoryEntry equippedItem = equippedItems[i];
			EquippedConsumableSlotButton button = Instantiate(equippedConsumableButtonPrefab).GetComponent<EquippedConsumableSlotButton>();
			button.Init(equippedItem, this, i);
			button.gameObject.transform.SetParent(equippedConsumableListContent);
			button.gameObject.transform.localScale = Vector3.one;
		}
	}
	public void PopulateEquippedTraitsList(UpcomingLifeTraits equippedLarvaTraits, UpcomingLifeTraits equippedPupaTraits) {
		foreach(Transform child in equippedTraitListLarvaContent) {
			Destroy(child.gameObject);
		}
		foreach(Transform child in equippedTraitListPupaContent) {
			Destroy(child.gameObject);
		}
		// note that we display all upcoming pupa traits, both passive and active,
		// but only display 1 of those for the larva

		// TODO: DRY
		for (int i = 0; i < equippedLarvaTraits.passiveTraits.Length; i++) {
			UpcomingLifeTrait trait = equippedLarvaTraits.passiveTraits[i];
			Debug.Log("trait: "+trait);
			Debug.Log(trait == null);
;			EquippedTraitButton button = Instantiate(equippedTraitButtonPrefab).GetComponent<EquippedTraitButton>();
			button.Init(trait, this, i, equippedLarvaTraits, TraitType.Passive);
			button.gameObject.transform.SetParent(equippedTraitListLarvaContent);
			button.gameObject.transform.localScale = Vector3.one;
		}
		for (int i = 0; i < equippedPupaTraits.passiveTraits.Length; i++) {
			UpcomingLifeTrait trait = equippedPupaTraits.passiveTraits[i];
			DisabledEquippedTraitButton button = Instantiate(disabledEquippedTraitButtonPrefab).GetComponent<DisabledEquippedTraitButton>();
			button.Init(trait, this, i, equippedPupaTraits, TraitType.Passive);
			button.gameObject.transform.SetParent(equippedTraitListPupaContent);
			button.gameObject.transform.localScale = Vector3.one;
		}
		for (int i = 0; i < equippedPupaTraits.activeTraits.Length; i++) {
			UpcomingLifeTrait trait = equippedPupaTraits.activeTraits[i];
			EquippedTraitButton button = Instantiate(equippedTraitButtonPrefab).GetComponent<EquippedTraitButton>();
			button.Init(trait, this, i, equippedPupaTraits, TraitType.Active);
			button.gameObject.transform.SetParent(equippedTraitListPupaContent);
			button.gameObject.transform.localScale = Vector3.one;
		}
	}

	public void EquipItem(InventoryEntry itemToEquip, int? slot) {
		PlayerController playerController = GameMaster.Instance.GetPlayerController();
		if (playerController != null) {
			switch (itemToEquip.type) {
				case InventoryItemType.Weapon:
					playerController.EquipWeapon(itemToEquip);
					break;
				case InventoryItemType.Consumable:
            		int slotValue = slot.GetValueOrDefault();
					playerController.EquipConsumableToSlot(itemToEquip.itemId, slotValue);
					CloseEquipConsumableMenu();
					break;
			}	
		}
	}

	// equip the appropriate Trait 
	// on the appropriate InventoryEntry 
	// to the appropriate slot 
	// in the appropriate UpcomingLifeTraits
	public void EquipTraitItem(InventoryEntry itemToEquip, int slot, UpcomingLifeTraits traitsToEquipTo, TraitType type) {
		Debug.Log("EQUIP TRAIT BUTTON");
		PlayerController playerController = GameMaster.Instance.GetPlayerController();
		if (playerController != null) {
			playerController.EquipTrait(itemToEquip, slot, traitsToEquipTo, type);
			CloseEquipTraitMenu();
		}
	}

	public void OpenEquipConsumableMenu(int slotNumber) {
		PlayerController playerController = GameMaster.Instance.GetPlayerController();
		if (playerController != null) {
            Inventory inventory = playerController.GetComponent<Inventory>();
			PopulateEquippableItemList(inventory.GetAllItemsOfType(InventoryItemType.Consumable), consumableListContent, slotNumber);
		}
		equippedConsumableList.gameObject.SetActive(false);
		consumableList.gameObject.SetActive(true);
	}
	public void PopulateEquippableItemList(List<InventoryEntry> items, Transform contentParent, int? slot) {
		foreach(Transform child in contentParent) {
			Destroy(child.gameObject);
		}
		foreach (InventoryEntry item in items) {
			ItemButton button = Instantiate(itemButtonPrefab).GetComponent<ItemButton>();
			button.Init(item, this, slot);
			button.gameObject.transform.SetParent(contentParent);
			button.gameObject.transform.localScale = Vector3.one;
		}
	}

	// make both UpcomingLife menus inactive
	// PopulateEquippableItemList
	public void OpenEquipTraitMenu(int slotNumber, UpcomingLifeTraits traits, TraitType type) {
		PlayerController playerController = GameMaster.Instance.GetPlayerController();
		if (playerController != null) {
            Inventory inventory = playerController.GetComponent<Inventory>();
			PopulateEquippableTraitList(inventory.GetAllItemsOfType(InventoryItemType.Trait), traitListContent, slotNumber, traits, type);
		}
		equippedTraitListLarva.gameObject.SetActive(false);
		equippedTraitListPupa.gameObject.SetActive(false);
		traitList.gameObject.SetActive(true);
	}

	
	public void PopulateEquippableTraitList(List<InventoryEntry> items, Transform contentParent, int? slot, UpcomingLifeTraits lifeTraits, TraitType type) {
		foreach(Transform child in contentParent) {
			Destroy(child.gameObject);
		}
		foreach (InventoryEntry item in items) {
			Debug.Log("equippable trait: "+item.ToString());
			TraitButton button = Instantiate(traitButtonPrefab).GetComponent<TraitButton>();
			button.Init(item, this, slot, lifeTraits, type);
			button.gameObject.transform.SetParent(contentParent);
			button.gameObject.transform.localScale = Vector3.one;
		}
	}

	public void CloseEquipConsumableMenu() {
		PlayerController playerController = GameMaster.Instance.GetPlayerController();
		if (playerController != null) {
            Inventory inventory = playerController.GetComponent<Inventory>();
			PopulateEquippedConsumablesList(inventory.GetEquippedConsumableInventoryEntries());
		}
		consumableList.gameObject.SetActive(false);
		equippedConsumableList.gameObject.SetActive(true);
	}

	public void CloseEquipTraitMenu() {
		PlayerController playerController = GameMaster.Instance.GetPlayerController();
		if (playerController != null) {
            Inventory inventory = playerController.GetComponent<Inventory>();
			Debug.Log("upcoming larva: "+inventory.GetUpcomingLarva().ToString());
			Debug.Log("upcoming pupa: "+inventory.GetUpcomingPupa().ToString());
			PopulateEquippedTraitsList(inventory.GetUpcomingLarva(), inventory.GetUpcomingPupa());
		}
		traitList.gameObject.SetActive(false);
		equippedTraitListLarva.gameObject.SetActive(true);
		equippedTraitListPupa.gameObject.SetActive(true);
	}
}
