using UnityEngine.EventSystems;
// to equip a trait we need to know:
// which UpcomingLifeTraits we're equipping to
// which item we're representing
// which trait on the item - active or passive - we should show
public class TraitButton : ItemButton
{
  private TraitSlot traitSlot;
  public InventoryEntry itemEntry;


  public void Init(InventoryEntry itemEntryInfo, InventoryScreen parentScreen, TraitSlot ts)
  {
    base.Init(itemEntryInfo, parentScreen);
    traitSlot = ts;
    itemEntry = itemEntryInfo;
    nameLabel.text = itemEntry.itemName;
    if (itemEntryInfo.equipped) { nameLabel.text += "\n (Equipped)"; }
  }

  protected override void HandleClick()
  {
    inventoryScreen.EquipTraitItem(item, traitSlot);
  }

  public override void OnPointerEnter(PointerEventData data)
  {
    TraitSlotToTraitDictionary itemTraits = itemEntry.traits.EquippedTraits();
    Trait trait = itemTraits.ContainsKey(traitSlot)
      ? itemTraits[traitSlot]
      : null;
    string traitName = trait ? trait.traitName : "[None]";
    string traitDescription = trait ? trait.traitDescription : "";
    inventoryScreen.SetItemDescriptionText(
      itemEntry.itemName
      + "\n\n"
      + traitSlot + ": " + traitName
      + "\n\n"
      + traitDescription
      + "\n\n" + itemEntry.itemDescription
    );
    UnityEngine.Debug.Log("Pointer enter!!");
  }

  public override void OnPointerExit(PointerEventData data)
  {
    inventoryScreen.SetItemDescriptionText("");
    UnityEngine.Debug.Log("Pointer exit!!");
  }
}