using System;
using System.Collections.Generic;
using UnityEngine;

public class TileChangingSwitch : Interactable
{
  public Sprite defaultSprite;
  public Sprite flippedSprite;
  public GameObject tileTargets;
  private bool flipped = false;
  public override string interactableText {
    get {
      if (!flipped) {
        return "use switch";
      }
      return "";
    }
    set {}
  }

  void PlayerActivate () {
		// Replace each tile
    if (flipped) { return; }
    flipped = true;
    isInteractable = false;
    GetComponent<SpriteRenderer>().sprite = flippedSprite;
    foreach(TileTarget tileTarget in tileTargets.GetComponentsInChildren<TileTarget>()) {
      FloorLayer fl = (FloorLayer) Enum.Parse(typeof (FloorLayer), LayerMask.LayerToName(tileTarget.gameObject.layer));
      TileLocation loc = new TileLocation(tileTarget.transform.position, fl);
      GridManager.Instance.ReplaceTileAtLocation(loc, tileTarget.tileToReplaceWith);
    }
	}
}
