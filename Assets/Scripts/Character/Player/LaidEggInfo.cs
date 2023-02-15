using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaidEggInfo
{
  public TileLocation location;
  public TraitSlotToTraitDictionary slots;

  public LaidEggInfo(TileLocation tl, TraitSlotToTraitDictionary t)
  {
    location = tl;
    slots = t;
  }
}
