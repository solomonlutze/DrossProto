
using UnityEngine;
using UnityEngine.EventSystems;
// to equip a trait we need to know:
// which UpcomingLifeTraits we're equipping to
// which item we're representing
// which trait on the item - active or passive - we should show
public class TraitButton : MonoBehaviour
{
    private TraitSlot traitSlot;
    public Trait equippedTrait;
    public Trait itemTrait;
    public LymphLogo lymphLogo;
    public SuperTextMesh nameLabel;
    public SuperTextMesh slotNameLabel; // currently an adjacent object in TraitButtonWithHeader prefab; not present on TraitButton prefab
    AttributesView parentScreen;

    public void Init(TraitSlot ts, Trait et, Trait it, AttributesView ps)
    {
        // base.Init(itemEntryInfo, parentScreen);
        traitSlot = ts;
        equippedTrait = et;
        itemTrait = it;
        parentScreen = ps;
        nameLabel.text = et.traitName;
        slotNameLabel.text = ts.ToString();
        // if (itemEntryInfo.equipped) { nameLabel.text += "\n (Equipped)"; }
        if (lymphLogo != null) { lymphLogo.Init(et.lymphType); }
    }

    public void HandleClick()
    {
        // replace the currently-equipped trait with new trait
        // destroy TraitPickupItem
        // should maybe happen on parent screen?
        parentScreen.OnTraitButtonClicked(itemTrait, traitSlot);
        // GameMaster.Instance.GetPlayerController().EquipTrait(itemTrait, traitSlot);


        // inventoryScreen.EquipTraitItem(item, traitSlot);
    }

    public void OnPointerEnter(PointerEventData data)
    {

        // display difference between existing and new traits

        // TraitSlotToTraitDictionary itemTraits = itemEntry.traits.EquippedTraits();
        // Trait trait = itemTraits.ContainsKey(traitSlot)
        //   ? itemTraits[traitSlot]
        //   : null;
        // string traitName = trait ? trait.traitName : "[None]";
        // string traitDescription = trait ? trait.traitDescription : "";
        // inventoryScreen.SetItemDescriptionText(
        //   itemEntry.itemName
        //   + "\n\n"
        //   + traitSlot + ": " + traitName
        //   + "\n\n"
        //   + traitDescription
        //   + "\n\n" + itemEntry.itemDescription
        // );
        UnityEngine.Debug.Log("Pointer enter!!");
    }

    public void OnPointerExit(PointerEventData data)
    {
        // inventoryScreen.SetItemDescriptionText("");
        UnityEngine.Debug.Log("Pointer exit!!");
    }
}