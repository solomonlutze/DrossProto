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
  public SuperTextMesh itemNameText;


  Trait trait;
  TraitSlot traitSlot;

  public void Init(Trait t, TraitSlot ts, string itemName = null)
  {
    trait = t;
    traitSlot = ts;
    // Debug.Log("init'ing trait info for " + trait);
    if (trait == null)
    {
      gameObject.SetActive(false);
      return;
    }
    providingLymphNameText.text = trait.traitName;
    skillNameText.text = trait.skill.displayName;
    skillDescriptionText.text = trait.skill.description;
    if (itemName != null)
    {
      itemNameText.text = itemName;
    }
    else
    {
      itemNameText.text = "(No Change)";
    }
  }

}