using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
// to equip a trait we need to know:
// which UpcomingLifeTraits we're equipping to
// which item we're representing
// which trait on the item - active or passive - we should show
public class TraitButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
  private TraitSlot traitSlot;
  public Trait equippedTrait;
  public Trait itemTrait;
  public SuperTextMesh slotNameLabel; // currently an adjacent object in TraitButtonWithHeader prefab; not present on TraitButton prefab
  public GameObject owningObject; // root-level GO for a TraitButtonWithHeader; not present on TraitButton prefab
  public SuperTextMesh[] traitAttributeTexts;
  public TraitInfo currentTraitInfo;
  public TraitInfo nextTraitInfo;
  BugStatusView parentScreen;

  public void Init(TraitSlot ts, Trait et, Trait it, BugStatusView ps, Dictionary<CharacterAttribute, IAttributeDataInterface> attributeDataObjects)
  {
    // base.Init(itemEntryInfo, parentScreen);
    traitSlot = ts;
    equippedTrait = et;
    itemTrait = it;
    parentScreen = ps;
    currentTraitInfo.Init(et, ts, attributeDataObjects);
    if (it != null)
    {
      nextTraitInfo.gameObject.SetActive(true);
      nextTraitInfo.Init(it, ts, attributeDataObjects);
    }
    else
    {
      nextTraitInfo.gameObject.SetActive(false);
    }
    // nameLabel.text = et.traitName;
    slotNameLabel.text = ts.ToString();
    // SetTraitAttributeText(it);z
    // if (itemEntryInfo.equipped) { nameLabel.text += "\n (Equipped)"; }
    // if (lymphLogo != null) { lymphLogo.Init(et.lymphType); }
  }

  public void SetTraitAttributeText(Trait it)
  {
    Debug.LogError("wat?");
    if (it == null)
    {
      foreach (SuperTextMesh m in traitAttributeTexts)
      {
        m.gameObject.SetActive(false);
      }
      return;
    }
    int i = 0;
    if (it.attributeModifiers.Keys.Count > 4) { Debug.LogError("More attributes than text fields in TraitButton: " + it); }
    foreach (KeyValuePair<CharacterAttribute, int> entry in it.attributeModifiers)
    {
      traitAttributeTexts[i].gameObject.SetActive(true);
      traitAttributeTexts[i].text = entry.Key.ToString() + Enumerable.Repeat("+", entry.Value);
      i++;
    }
    while (i < traitAttributeTexts.Length)
    {
      traitAttributeTexts[i].gameObject.SetActive(false);
      i++;
    }
  }
  public void HandleClick()
  {
    // replace the currently-equipped trait with new trait
    // destroy TraitPickupItem
    // should maybe happen on parent screen?
    parentScreen.OnTraitButtonClicked(itemTrait, traitSlot);
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
    if (itemTrait == null) { return; }
    // parentScreen.ShowHighlightedTraitDelta(equippedTrait, itemTrait);
  }

  public void OnPointerExit(PointerEventData data)
  {
    // inventoryScreen.SetItemDescriptionText("");
    if (itemTrait == null) { return; }
    // parentScreen.UnshowHighlightedTraitDelta();
  }
}