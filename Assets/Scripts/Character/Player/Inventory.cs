using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum InventoryItemType { Currency, Weapon, Consumable, Trait, Food }

public class PickedUpItem
{
  public string itemId;
  public string itemName;
  public int quantity;
}
[System.Serializable]
public class InventoryEntry
{
  public string id;
  public bool equipped;
  public string itemId;
  public string itemName;
  public InventoryItemType type;
  public int quantity;
  public string guid;
  public string itemDescription;
}
public class TraitItemInventoryEntry : InventoryEntry
{
  public LymphType lymphType;

}
// Controls what a character is holding.
// Enemies proooobably won't have an inventory, they'll just have item lists.
// So this is effectively a player inventory.
public class Inventory : MonoBehaviour
{

  public List<PickupItem> initialItems;
  private Dictionary<string, InventoryEntry> inventory;
  public List<PickedUpItem> lastPickedUpItems;
  public string[] equippedItems;
  public int activeItemIndex = 0;
  public int numEquippableConsumables;
  public string lastUsedItem = null;
  public Character owner;
  public InventoryScreen inventoryScreen;
  public TraitSlotToUpcomingTraitDictionary upcomingPupa; // your next life
  public LymphTypeToIntDictionary lymphTypeCounts;

  // TODO: Some of this should get bumped into an Init that's called by PlayerController when we make a new character
  void Awake()
  {
    equippedItems = new string[numEquippableConsumables];
    lastPickedUpItems = new List<PickedUpItem>();
    inventory = new Dictionary<string, InventoryEntry>();
    // foreach (PickupItem item in initialItems)
    // {
    //   if (item != null)
    //   {
    //     AddToInventory(item);
    //   }
    // }
  }

  // void Update()
  // {
  //   if (Input.GetButtonDown("UseItem"))
  //   {
  //     UseActiveItem();
  //   }
  //   else if (Input.GetButtonDown("ActiveItemNext"))
  //   {
  //     AdvanceActiveEquippedItem();
  //   }
  // }

  // on stacking:
  // consumables and currencies go in stacks
  // weapons and trait items do not
  // if we're adding a consumable/currency to the inventory, we just look for its key and add it to the stack, if it exists.
  // if we're adding a weapon or trait, we don't check, but we DO generate a new GUID for it
  // when looking at equipped items, we check via guid.
  // I guess currencies/consumables are keyed by itemId, and weapons/traits are keyed by GUID?
  // that sounds bad but maybe it's fine

  public void AddToInventory(PickupItem item)
  {
    InventoryEntry entry;
    if (inventory.ContainsKey(item.itemId))
    {
      entry = inventory[item.itemId];
      entry.quantity += item.quantity;
    }
    else
    {
      // TODO: the below line is really unsafe!!
      ItemData itemInfo = (ItemData)(Resources.Load("Data/ItemData/" + item.itemType.ToString() + "/" + item.itemId) as ScriptableObject);
      TraitItemData traitItemInfo = (TraitItemData)itemInfo;
      if (traitItemInfo != null)
      {
        entry = new TraitItemInventoryEntry();
      }
      else
      {
        entry = new InventoryEntry();
      }
      if (itemInfo == null)
      {
        Debug.LogError("no item data found for " + item.itemId + " of type " + item.itemType);
      }
      entry.itemId = item.itemId;
      entry.quantity = item.quantity;
      entry.type = itemInfo.type;
      entry.itemName = itemInfo.itemName;
      entry.itemDescription = itemInfo.itemDescription;
      switch (item.itemType)
      {
        case InventoryItemType.Consumable:
        case InventoryItemType.Currency:
          inventory[item.itemId] = entry;
          break;
        case InventoryItemType.Trait:
          entry.guid = System.Guid.NewGuid().ToString("N");
          inventory[entry.guid] = entry;
          TraitItemInventoryEntry tEntry = (TraitItemInventoryEntry)entry;
          if (tEntry == null)
          {
            Debug.LogError("Trait Item can't be coerced to TraitItemData: " + itemInfo.itemName);
          }
          tEntry.lymphType = traitItemInfo.lymphType;
          break;
        case InventoryItemType.Weapon:
        default:
          entry.guid = System.Guid.NewGuid().ToString("N");
          inventory[entry.guid] = entry;
          break;

      }
      if (itemInfo.type == InventoryItemType.Consumable)
      {
        AutoEquipConsumable(item.itemId);
      }
    }
    PickedUpItem pui = new PickedUpItem();
    pui.itemId = item.itemId;
    pui.itemName = entry.itemName;
    pui.quantity = item.quantity;
    lastPickedUpItems.Add(pui);
  }

  public void ClearPickedUpItem()
  {
    if (lastPickedUpItems.Count > 0)
    {
      lastPickedUpItems.RemoveAt(0);
    }
  }

  public void RemoveFromInventory(string itemId, int quantity)
  {
    if (inventory.ContainsKey(itemId))
    {
      inventory[itemId].quantity = Mathf.Max(inventory[itemId].quantity - quantity, 0);
    }
  }

  public InventoryEntry GetInventoryEntry(string itemId)
  {
    if (inventory.ContainsKey(itemId))
    {
      return inventory[itemId];
    }
    return null;
  }

  void AutoEquipConsumable(string newItemId)
  {
    for (int i = 0; i < equippedItems.Length; i++)
    {
      string equippedItem = equippedItems[i];
      if (equippedItem == null || equippedItem == "")
      {
        equippedItems[i] = newItemId;
        break;
      }
    }
  }

  public void EquipConsumableToSlot(string itemId, int slot)
  {
    for (int i = 0; i < equippedItems.Length; i++)
    {
      if (i == slot)
      {
        equippedItems[i] = itemId;
      }
      else if (equippedItems[i] == itemId)
      {
        equippedItems[i] = null;
      }
    }
  }

  public LymphTypeToIntDictionary GetLymphTypeCounts(TraitSlotToUpcomingTraitDictionary futureTraits)
  {
    LymphTypeToIntDictionary lymphTypeCounts = new LymphTypeToIntDictionary();
    foreach (UpcomingLifeTrait upcomingLifeTrait in futureTraits.Values)
    {
      lymphTypeCounts[upcomingLifeTrait.lymphType]++;
    }
    return lymphTypeCounts;
  }

  public void UnequipTraitItemInSlot(TraitSlot slot)
  {
    UpcomingLifeTrait lifeTraitToUnequip = upcomingPupa[slot];
    upcomingPupa[slot] = new UpcomingLifeTrait(null, LymphType.None, null);
    if (lifeTraitToUnequip != null && lifeTraitToUnequip.inventoryItem != null)
    {
      lifeTraitToUnequip.inventoryItem.equipped = false;
      lymphTypeCounts[lifeTraitToUnequip.lymphType]--;
    }
  }

  public void UnequipTraitItemByGuid(string traitItemGuid)
  {
    List<TraitSlot> keys = new List<TraitSlot>(upcomingPupa.Keys);
    foreach (TraitSlot s in keys)
    {
      if (
        upcomingPupa[s] != null &&
        upcomingPupa[s].inventoryItem != null &&
        upcomingPupa[s].inventoryItem.guid == traitItemGuid
      )
      {
        UnequipTraitItemInSlot(s);
      }
    }
  }

  public InventoryEntry[] GetEquippedConsumableInventoryEntries()
  {
    return equippedItems.Select(entry => entry == null ? null : GetInventoryEntry(entry)).ToArray();
  }

  public TraitSlotToUpcomingTraitDictionary GetUpcomingPupa()
  {
    Debug.Log("upcoming pupa: " + upcomingPupa);
    return upcomingPupa;
  }

  // increase activeItemIndex and loop to 0 if it's greater than the length of our equippedItems list
  public void AdvanceActiveEquippedItem()
  {
    if (equippedItems.Length > 0)
    {
      for (int i = 0; i < equippedItems.Length; i++)
      {
        activeItemIndex = (int)Mathf.Repeat(activeItemIndex + 1, equippedItems.Length);
        if (equippedItems[activeItemIndex] != null) { break; }
      }
      Debug.Log("Equipped item: " + equippedItems[activeItemIndex]);
    }
  }

  public void UseActiveItem()
  {
    if (activeItemIndex < equippedItems.Length)
    {
      UseItem(equippedItems[activeItemIndex]);
    }
  }

  public void UseItem(string itemId)
  {
    InventoryEntry entry = inventory[itemId];
    if (entry.quantity <= 0)
    {
      Debug.LogError("No item quantity in inventory! Cannot use!");
      return;
    }
    ConsumableItemData itemInfo = (Resources.Load("Data/ItemData/" + entry.type + "/" + itemId) as ConsumableItemData);
    // itemInfo.Init(owner);
    Debug.Log("using " + itemInfo.itemName);
    itemInfo.Use(owner);
    lastUsedItem = itemInfo.itemName;
  }

  public void MarkItemEquipped(string itemId)
  {
    InventoryEntry entry = GetInventoryEntry(itemId);
    if (entry != null)
    {
      entry.equipped = true;
    }
  }
  public void MarkItemUnequipped(string itemId)
  {
    InventoryEntry entry = GetInventoryEntry(itemId);
    if (entry != null)
    {
      Debug.Log("marking item unequipped: " + itemId);
      entry.equipped = false;
    }
  }
  // called each time you respawn
  public void AdvanceUpcomingLifeTraits(TraitSlotToUpcomingTraitDictionary previousPupa)
  {
    owner.AssignTraitsForNextLife(previousPupa == null ? new TraitSlotToUpcomingTraitDictionary() : previousPupa);
    upcomingPupa = CreateNewUpcomingTraits();
  }

  private TraitSlotToUpcomingTraitDictionary CreateNewUpcomingTraits()
  {
    // eventually this could assign default traits based on spawn region
    return new TraitSlotToUpcomingTraitDictionary();
  }

  public List<InventoryEntry> GetAllItemsOfType(InventoryItemType type)
  {
    return inventory.Values.Where(val => val.type == type).ToList();
  }
}