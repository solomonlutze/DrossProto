using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum InventoryItemType {Currency, Weapon, Consumable, Trait}

public class PickedUpItem {
    public string itemId;
    public string itemName;
    public int quantity;
}
public class InventoryEntry {
    public string id;
    public bool equipped;
    public string itemId;
    public string itemName;
    public InventoryItemType type;
    public int quantity;
    public string guid;
}
// Controls what a character is holding.
// Enemies proooobably won't have an inventory, they'll just have item lists.
// So this is effectively a player inventory.
public class Inventory : MonoBehaviour {

    public List<PickupItem> initialItems;
    private Dictionary<string, InventoryEntry> inventory;
    public List<PickedUpItem> lastPickedUpItems;
    public string[] equippedItems;
    public int activeItemIndex = 0;
    public int numEquippableConsumables;
    public string lastUsedItem = null;
    public Character owner;
    public InventoryScreen inventoryScreen;

    public UpcomingLifeTraits upcomingLarva; // two lives from now
    public UpcomingLifeTraits upcomingPupa; // your next life

    public int defaultNumPassiveTraits = 2;
    public int defaultNumActiveTraits = 2;

    // TODO: Some of this should get bumped into an Init that's called by PlayerController when we make a new character
    void Awake() {
        equippedItems = new string[numEquippableConsumables];
        lastPickedUpItems = new List<PickedUpItem>();
        inventory = new Dictionary<string, InventoryEntry>();
        foreach (PickupItem item in initialItems) {
            if (item != null) {
                AddToInventory(item);
            }
        }
    }

    void Update() {
        if (Input.GetButtonDown("UseItem")) {
            UseActiveItem();
        }
        else if (Input.GetButtonDown("ActiveItemNext")) {
            AdvanceActiveEquippedItem();
        }
    }

    // on stacking:
    // consumables and currencies go in stacks
    // weapons and trait items do not
    // if we're adding a consumable/currency to the inventory, we just look for its key and add it to the stack, if it exists.
    // if we're adding a weapon or trait, we don't check, but we DO generate a new GUID for it
    // when looking at equipped items, we check via guid.
    // I guess currencies/consumables are keyed by itemId, and weapons/traits are keyed by GUID?
    // that sounds bad but maybe it's fine

    public void AddToInventory(PickupItem item) {
        InventoryEntry entry = new InventoryEntry();
        if (inventory.ContainsKey(item.itemId)){
            entry = inventory[item.itemId];
            entry.quantity += item.quantity;
        } else {
            // TODO: the below line is really unsafe!!
            ItemData itemInfo = (ItemData) (Resources.Load("Data/ItemData/"+item.itemType.ToString() + "/"+item.itemId) as ScriptableObject);
            entry.itemId = item.itemId;
            entry.quantity = item.quantity;
            entry.type = itemInfo.type;
            entry.itemName = itemInfo.itemName;
            switch (item.itemType) {
                case InventoryItemType.Consumable:
                case InventoryItemType.Currency:
                    inventory[item.itemId] = entry;
                    break;
                case InventoryItemType.Trait:
                case InventoryItemType.Weapon:
                default:
                    entry.guid = System.Guid.NewGuid().ToString("N");
                    inventory[entry.guid] = entry;
                    break;

            }
            if (itemInfo.type == InventoryItemType.Consumable) {
                AutoEquipConsumable(item.itemId);
            }
        }
        PickedUpItem pui = new PickedUpItem();
        pui.itemId = item.itemId;
        pui.itemName = entry.itemName;
        pui.quantity = item.quantity;
        Debug.Log(pui.itemId);
        Debug.Log(pui.itemName);
        Debug.Log(pui.quantity);
        lastPickedUpItems.Add(pui);
    }

    public void ClearPickedUpItem() {
        if (lastPickedUpItems.Count > 0) {
            lastPickedUpItems.RemoveAt(0);
        }
    }

    public void RemoveFromInventory(string itemId, int quantity) {
        if (inventory.ContainsKey(itemId)){
            inventory[itemId].quantity = Mathf.Max(inventory[itemId].quantity - quantity, 0);
        }
    }

    public InventoryEntry GetInventoryEntry(string itemId) {
        if (inventory.ContainsKey(itemId)){
            return inventory[itemId];
        }
        return null;
    }

    void AutoEquipConsumable(string newItemId) {
        for (int i = 0; i < equippedItems.Length; i++) {
            string equippedItem = equippedItems[i];
            if (equippedItem == null || equippedItem == "") {
                equippedItems[i] = newItemId;
                break;
            }
        }
    }

    public void EquipConsumableToSlot(string itemId, int slot) {
            for (int i = 0; i < equippedItems.Length; i++) {
            if (i == slot) {
                equippedItems[i] = itemId;
            } else if (equippedItems[i] == itemId) {
                equippedItems[i] = null;
            }
        }
    }

    public void EquipTraitToUpcomingLifeTrait(InventoryEntry itemToEquip, int slot, UpcomingLifeTraits traitsToEquipTo, TraitType type) {
        UpcomingLifeTrait[] traitsList;
        string traitName;
        TraitItemData itemInfo = (Resources.Load("Data/ItemData/Trait/"+itemToEquip.itemId) as GameObject).GetComponent<TraitItemData>();
        UnequipTraitItem(itemToEquip.guid);
        switch(type) {
            case TraitType.Passive:
                traitsList = traitsToEquipTo.passiveTraits;
                traitName = itemInfo.passiveTrait;
                break;
            case TraitType.Active:
            default:
                traitsList = traitsToEquipTo.activeTraits;
                traitName = itemInfo.activeTrait;
                break;
        }
        traitsList[slot] = new UpcomingLifeTrait(traitName, itemToEquip);
    }

    // TODO: DRY
    public void UnequipTraitItem(string traitItemGuid) {
        Debug.Log("traitItemGuid: "+traitItemGuid);
        for (int i = 0; i < upcomingLarva.activeTraits.Length; i++) {
            UpcomingLifeTrait trait = upcomingLarva.activeTraits[i];
            if (trait != null) {
                Debug.Log("guid comparing against: "+trait.inventoryItem.guid);
            }
            if (trait != null && trait.inventoryItem.guid == traitItemGuid) {
                upcomingLarva.activeTraits[i] = null;
            }
        }
        for (int i = 0; i < upcomingLarva.passiveTraits.Length; i++) {
            UpcomingLifeTrait trait = upcomingLarva.passiveTraits[i];
            if (trait != null) {
                Debug.Log("guid comparing against: "+trait.inventoryItem.guid);
            }
            if (trait != null && trait.inventoryItem.guid == traitItemGuid) {
                upcomingLarva.passiveTraits[i] = null;
            }
        }
        for (int i = 0; i < upcomingPupa.activeTraits.Length; i++) {
            UpcomingLifeTrait trait = upcomingPupa.activeTraits[i];
            if (trait != null) {
                Debug.Log("guid comparing against: "+trait.inventoryItem.guid);
            }
            if (trait != null && trait.inventoryItem.guid == traitItemGuid) {
                upcomingPupa.activeTraits[i] = null;
            }
        }
        for (int i = 0; i < upcomingPupa.passiveTraits.Length; i++) {
            UpcomingLifeTrait trait = upcomingPupa.passiveTraits[i];
            if (trait != null) {
                Debug.Log("guid comparing against: "+trait.inventoryItem.guid);
            }
            if (trait != null && trait.inventoryItem.guid == traitItemGuid) {
                upcomingPupa.passiveTraits[i] = null;
            }
        }
    }
    public InventoryEntry[] GetEquippedConsumableInventoryEntries() {
        return equippedItems.Select(entry => entry == null ? null : GetInventoryEntry(entry)).ToArray();
    }
    public UpcomingLifeTraits GetUpcomingLarva() {
        return upcomingLarva;
    }

    public UpcomingLifeTraits GetUpcomingPupa() {
        return upcomingPupa;
    }

    // increase activeItemIndex and loop to 0 if it's greater than the length of our equippedItems list
    public void AdvanceActiveEquippedItem() {
        if (equippedItems.Length > 0) {
            for (int i = 0; i < equippedItems.Length; i++) {
                activeItemIndex = (int) Mathf.Repeat(activeItemIndex + 1, equippedItems.Length);
                if (equippedItems[activeItemIndex] != null) { break; }
            }
            Debug.Log("Equipped item: "+equippedItems[activeItemIndex]);
        }
    }

    public void UseActiveItem() {
        if (activeItemIndex < equippedItems.Length) {
            UseItem(equippedItems[activeItemIndex]);
        }
    }

    public void UseItem(string itemId) {
        InventoryEntry entry = inventory[itemId];
        if (entry.quantity <= 0) {
            Debug.LogError ("No item quantity in inventory! Cannot use!");
            return;
        }
        ConsumableItemData itemInfo = (Resources.Load("Data/ItemData/"+entry.type+"/"+itemId) as ConsumableItemData);
        // itemInfo.Init(owner);
        Debug.Log("using "+itemInfo.itemName);
        itemInfo.Use(owner);
        lastUsedItem = itemInfo.itemName;
    }

    public void MarkItemEquipped(string itemId) {
        InventoryEntry entry = GetInventoryEntry(itemId);
        if (entry != null) {
            entry.equipped = true;
        }
    }
    public void MarkItemUnequipped(string itemId) {
        InventoryEntry entry = GetInventoryEntry(itemId);
        if (entry != null) {
            Debug.Log("marking item unequipped: "+itemId);
            entry.equipped = false;
        }
    }
    // called each time you respawn
    public void AdvanceUpcomingLifeTraits(UpcomingLifeTraits previousLarva, UpcomingLifeTraits previousPupa) {
        owner.AssignTraitsForNextLife(previousPupa == null ? CreateNewUpcomingTraits() : previousPupa);
        upcomingPupa = previousLarva == null ? CreateNewUpcomingTraits() : previousLarva;
        upcomingLarva = CreateNewUpcomingTraits();
    }

    private UpcomingLifeTraits CreateNewUpcomingTraits() {
    // eventually this could assign default traits based on spawn region
        return new UpcomingLifeTraits(defaultNumPassiveTraits, defaultNumActiveTraits);
    }

    public List<InventoryEntry> GetAllItemsOfType(InventoryItemType type) {
        return inventory.Values.Where(val => val.type == type).ToList();
    }
}