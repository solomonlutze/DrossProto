using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
// to equip a trait we need to know:
// which UpcomingLifeTraits we're equipping to
// which item we're representing
// which trait on the item - active or passive - we should show
public class TraitInfo : MonoBehaviour
{
  // private TraitSlot traitSlot;
  // public Trait trait;

  public LymphLogo lymphLogoObject;
  public SuperTextMesh nameLabelObject;
  public SuperTextMesh[] traitAttributeTexts;

  public void Init(Trait trait, TraitSlot traitSlot, Dictionary<CharacterAttribute, IAttributeDataInterface> attributeDataObjects)
  {
    Debug.Log("init'ing trait info for " + trait);
    if (trait == null)
    {
      gameObject.SetActive(false);
      return;
    }
    // base.Init(itemEntryInfo, parentScreen);
    nameLabelObject.text = trait.traitName;
    SetTraitAttributeText(trait, traitSlot, attributeDataObjects);
    // if (itemEntryInfo.equipped) { nameLabel.text += "\n (Equipped)"; }
    if (lymphLogoObject != null) { lymphLogoObject.Init(trait.lymphType); }
  }

  public void SetTraitAttributeText(Trait trait, TraitSlot traitSlot, Dictionary<CharacterAttribute, IAttributeDataInterface> attributeDataObjects)
  {
    int i = 0;
    if (trait.attributeModifiers.Keys.Count > 4) { Debug.LogError("More attributes than text fields in TraitButton: " + trait); }
    foreach (KeyValuePair<CharacterAttribute, int> entry in trait.attributeModifiers)
    {
      traitAttributeTexts[i].gameObject.SetActive(true);
      string attributeName = attributeDataObjects[entry.Key].displayNameWithGroup;
      traitAttributeTexts[i].text = $"{attributeName}{System.String.Concat(Enumerable.Repeat("+", entry.Value - 1))}";
      i++;
    }
    while (i < traitAttributeTexts.Length)
    {
      traitAttributeTexts[i].gameObject.SetActive(false);
      i++;
    }
  }

}