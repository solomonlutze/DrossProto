using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraitPickupItem : PickupItem
{

  public TraitSlotToTraitDictionary traits;
  public override string interactableText
  {
    get { return "Collect"; }
    set { }
  }

  void PlayerActivate()
  {
    if (GameMaster.Instance.GetPlayerController() != null)
    {
      GameMaster.Instance.canvasHandler.DisplayBugStatusViewForTraitItem(this);
    }
  }

}
