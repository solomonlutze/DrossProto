using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
// to equip a trait we need to know:
// which UpcomingLifeTraits we're equipping to
// which item we're representing
// which trait on the item - active or passive - we should show
public class BugTraitInfo : MonoBehaviour
{
  // private TraitSlot traitSlot;
  // public Trait trait;

  public SuperTextMesh providingLymphNameText;
  public SuperTextMesh skillNameText;
  public SuperTextMesh skillDescriptionText;

  public void Init(Trait trait)
  {
    Debug.Log("init'ing trait info for " + trait);
    if (trait == null)
    {
      gameObject.SetActive(false);
      return;
    }
    providingLymphNameText.text = trait.traitName;
    skillNameText.text = trait.skill.displayName;
    skillDescriptionText.text = trait.skill.description;
    // // base.Init(itemEntryInfo, parentScreen);
    // nameLabelObject.text = trait.traitName;
    // SetTraitAttributeText(trait, traitSlot, attributeDataObjects);
    // // if (itemEntryInfo.equipped) { nameLabel.text += "\n (Equipped)"; }
    // if (lymphLogoObject != null) { lymphLogoObject.Init(trait.lymphType); }
  }

}