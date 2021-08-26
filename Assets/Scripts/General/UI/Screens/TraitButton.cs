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
  public TraitSlot traitSlot;
  public BugStatusView parentScreen;

  public void HandleClick()
  {
    // replace the currently-equipped trait with new trait
    // destroy TraitPickupItem
    // should maybe happen on parent screen?
    parentScreen.OnTraitButtonClicked(traitSlot);
  }

  public void OnPointerEnter(PointerEventData data)
  {
    Debug.Log("pointer enter " + traitSlot);
    parentScreen.OnTraitButtonSelected(traitSlot);
  }

  public void OnPointerExit(PointerEventData data)
  {
    parentScreen.OnTraitButtonDeselected();
  }
}