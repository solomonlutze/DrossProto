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

public class InventoryScreen : MonoBehaviour
{

  public Transform weaponsListContent;
  public GameObject itemButtonPrefab;
  public GameObject traitButtonPrefab;

  public Transform consumableList;
  public SuperTextMesh consumableListHeader;
  public Transform consumableListContent;

  public Transform equippedConsumableList;
  public Transform equippedConsumableListContent;
  public GameObject equippedConsumableButtonPrefab;

  public Transform equippedTraitListPupa;
  public Transform equippedTraitListPupaContent;
  public GameObject equippedTraitButtonPrefab;
  public GameObject disabledEquippedTraitButtonPrefab;


  public SuperTextMesh tooltipText;


  public Transform tooltipPanel;
  public Transform traitList;
  public Transform traitListContent;

  void Awake()
  {
    // consumableList.gameObject.SetActive(false);
    tooltipPanel.gameObject.SetActive(false);
    traitList.gameObject.SetActive(false);
  }

  // Update is called once per frame
  void Update()
  {
  }

  public void Enable()
  {
    PlayerController playerController = GameMaster.Instance.GetPlayerController();
    if (playerController != null)
    {
      // CloseEquipConsumableMenu();
      CloseEquipTraitMenu();

      Inventory inventory = playerController.GetComponent<Inventory>();
      TraitSlotToUpcomingTraitDictionary upcomingPupa = inventory.GetUpcomingPupa();
      // PopulateEquippedConsumablesList(inventory.GetEquippedConsumableInventoryEntries());
      PopulateEquippedTraitsList(upcomingPupa);
    }
  }

  public void PopulateEquippedConsumablesList(InventoryEntry[] equippedItems)
  {
    foreach (Transform child in equippedConsumableListContent)
    {
      Destroy(child.gameObject);
    }
    for (int i = 0; i < equippedItems.Length; i++)
    {
      InventoryEntry equippedItem = equippedItems[i];
      EquippedConsumableSlotButton button = Instantiate(equippedConsumableButtonPrefab).GetComponent<EquippedConsumableSlotButton>();
      button.Init(equippedItem, this, i);
      button.gameObject.transform.SetParent(equippedConsumableListContent);
      button.gameObject.transform.localScale = Vector3.one;
    }
  }
  public void PopulateEquippedTraitsList(TraitSlotToUpcomingTraitDictionary equippedPupaTraits)
  {

    // foreach(Transform child in equippedTraitListPupaContent) {
    // 	Destroy(child.gameObject);
    // }
    for (int i = 0; i < equippedTraitListPupa.childCount; i++)
    {
      Transform equippedTraitDisplay = equippedTraitListPupa.GetChild(i);
      Debug.Log(equippedTraitDisplay.gameObject);
      EquippedTraitButton b = equippedTraitDisplay.GetComponent<EquippedTraitButton>();
      Debug.Log(b);
      b.Init(equippedPupaTraits[(TraitSlot)i], this, (TraitSlot)i);
    }
    // foreach (TraitSlot s in equippedPupaTraits.Keys) {
    // 	UpcomingLifeTrait trait = equippedPupaTraits[s];
    // 	EquippedTraitButton button = Instantiate(equippedTraitButtonPrefab, equippedTraitListPupaContent).GetComponent<EquippedTraitButton>();
    // 	button.Init(trait, this, s);
    // button.gameObject.transform.SetParent(equippedTraitListPupaContent);
    // button.gameObject.transform.localScale = Vector3.one;
    // LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    // for (int i = 0; i < equippedPupaTraits.activeTraits.Length; i++) {
    // 	UpcomingLifeTrait trait = equippedPupaTraits.activeTraits[i];
    // 	EquippedTraitButton button = Instantiate(equippedTraitButtonPrefab).GetComponent<EquippedTraitButton>();
    // 	button.Init(trait, this, i, equippedPupaTraits, TraitType.Active);
    // 	button.gameObject.transform.SetParent(equippedTraitListPupaContent);
    // 	button.gameObject.transform.localScale = Vector3.one;
    // }
  }

  public void EquipItem(InventoryEntry itemToEquip, int? slot)
  {
    PlayerController playerController = GameMaster.Instance.GetPlayerController();
    if (playerController != null)
    {
      switch (itemToEquip.type)
      {
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
  public void EquipTraitItem(InventoryEntry itemToEquip, TraitSlot slot)
  {
    PlayerController playerController = GameMaster.Instance.GetPlayerController();
    if (playerController != null)
    {
      playerController.EquipTrait(itemToEquip, slot);
      CloseEquipTraitMenu();
    }
  }

  public void OpenEquipConsumableMenu(int slotNumber)
  {
    PlayerController playerController = GameMaster.Instance.GetPlayerController();
    if (playerController != null)
    {
      Inventory inventory = playerController.GetComponent<Inventory>();
      PopulateEquippableItemList(inventory.GetAllItemsOfType(InventoryItemType.Consumable), consumableListContent, slotNumber);
    }
    equippedConsumableList.gameObject.SetActive(false);
    consumableList.gameObject.SetActive(true);
  }
  public void PopulateEquippableItemList(List<InventoryEntry> items, Transform contentParent, int slot)
  {
    foreach (Transform child in contentParent)
    {
      Destroy(child.gameObject);
    }
    foreach (InventoryEntry item in items)
    {
      ItemButton button = Instantiate(itemButtonPrefab).GetComponent<ItemButton>();
      button.Init(item, this, slot);
      button.gameObject.transform.SetParent(contentParent);
      button.gameObject.transform.localScale = Vector3.one;
    }
  }

  // make both UpcomingLife menus inactive
  // PopulateEquippableItemList
  public void OpenEquipTraitMenu(TraitSlot slot)
  {
    PlayerController playerController = GameMaster.Instance.GetPlayerController();
    tooltipPanel.gameObject.SetActive(true);
    if (playerController != null)
    {
      Inventory inventory = playerController.GetComponent<Inventory>();
      PopulateEquippableTraitList(inventory.GetAllItemsOfType(InventoryItemType.Trait), traitListContent, slot);
    }
    equippedTraitListPupa.gameObject.SetActive(false);
    traitList.gameObject.SetActive(true);
  }


  public void PopulateEquippableTraitList(List<InventoryEntry> items, Transform contentParent, TraitSlot slot)
  {
    foreach (Transform child in contentParent)
    {
      Destroy(child.gameObject);
    }
    foreach (InventoryEntry item in items)
    {
      TraitButton button = Instantiate(traitButtonPrefab).GetComponent<TraitButton>();
      button.Init((TraitItemInventoryEntry)item, this, slot);
      button.gameObject.transform.SetParent(contentParent);
      button.gameObject.transform.localScale = Vector3.one;
    }
  }

  public void CloseEquipConsumableMenu()
  {
    PlayerController playerController = GameMaster.Instance.GetPlayerController();
    if (playerController != null)
    {
      Inventory inventory = playerController.GetComponent<Inventory>();
      PopulateEquippedConsumablesList(inventory.GetEquippedConsumableInventoryEntries());
    }
    consumableList.gameObject.SetActive(false);
    equippedConsumableList.gameObject.SetActive(true);
  }

  public void CloseEquipTraitMenu()
  {
    PlayerController playerController = GameMaster.Instance.GetPlayerController();
    if (playerController != null)
    {
      Inventory inventory = playerController.GetComponent<Inventory>();
      PopulateEquippedTraitsList(inventory.GetUpcomingPupa());
    }
    SetItemDescriptionText("");
    tooltipPanel.gameObject.SetActive(false);
    traitList.gameObject.SetActive(false);
    equippedTraitListPupa.gameObject.SetActive(true);
  }

  public void SetItemDescriptionText(string newText)
  {
    tooltipText.text = newText;
  }
}
