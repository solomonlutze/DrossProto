using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
// to equip a trait we need to know:
// which UpcomingLifeTraits we're equipping to
// which item we're representing
// which trait on the item - active or passive - we should show
public class TraitButton : ButtonBase, ISelectHandler
{
  public TraitSlot traitSlot;
  public BugStatusView parentScreen_OLD;
  public EquipTraitsView parentScreen;

  public void HandleClick()
  {
    parentScreen.OnTraitButtonClicked(traitSlot);
  }
  public void OnSelect(BaseEventData data)
  {
    parentScreen.OnTraitButtonSelected(traitSlot);
  }

}