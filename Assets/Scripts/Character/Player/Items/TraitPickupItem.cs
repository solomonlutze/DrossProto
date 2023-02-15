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

  public override void PlayerActivate()
  {
    if (GameMaster.Instance.GetPlayerController() != null)
    {
      GameMaster.Instance.AddCollectedTraitItem(traits);
      // string itemName = "Lymph";
      foreach (TraitSlot slot in traits.Keys)
      {
        if (traits[slot] != null)
        {
          // itemName = slot.ToString();
          GameMaster.Instance.playerHud.SetPickupItem(traits[slot].traitName + " " + slot);
          break;
        }
      }
      Destroy(gameObject);
    }
  }

}
