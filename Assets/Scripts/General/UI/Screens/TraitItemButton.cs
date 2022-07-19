using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TraitItemButton : ButtonBase, ISelectHandler
{
  public Trait trait;
  public TraitSlot traitSlot;
  public SuperTextMesh itemName;
  public SuperTextMesh skillName;
  public EquipTraitsView parentScreen;

  public void Init(Trait t, TraitSlot slot, EquipTraitsView screen)
  {
    trait = t;
    if (trait != null)
    {
      skillName.text = trait.skill.displayName;
      itemName.text = trait.traitName + " " + slot;
    }
    traitSlot = slot;
    parentScreen = screen;
  }
  public void HandleClick()
  {
    parentScreen.OnTraitItemClicked(trait, traitSlot);
  }
  public void OnSelect(BaseEventData data)
  {
    parentScreen.OnTraitItemButtonSelected(trait, traitSlot);
  }

}