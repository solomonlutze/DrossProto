using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
// to equip a trait we need to know:
// which UpcomingLifeTraits we're equipping to
// which item we're representing
// which trait on the item - active or passive - we should show
public class SelectStartingBugButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler
{
  public StartingBugSelectScreen parentScreen;
  public int idx;
  public SuperTextMesh text;
  public Button button;
  public void Awake()
  {
    button.onClick.AddListener(SelectBug);
  }

  public void SelectBug()
  {
    parentScreen.SelectBug(idx);
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
    parentScreen.HighlightBug(idx);
  }
  public void OnSelect(BaseEventData data)
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
    parentScreen.HighlightBug(idx);
  }

  public void OnPointerExit(PointerEventData data)
  {
    // inventoryScreen.SetItemDescriptionText("");
    parentScreen.UnhighlightBug();
    // parentScreen.UnshowHighlightedTraitDelta();
  }

}