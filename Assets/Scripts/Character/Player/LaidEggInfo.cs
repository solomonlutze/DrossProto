using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaidEggInfo
{
  public TileLocation location;
  public TraitSlotToTraitDictionary traits;
  public GameObject eggInstance;

  public LaidEggInfo(TileLocation tl, TraitSlotToTraitDictionary t, GameObject e)
  {
    location = tl;
    traits = new TraitSlotToTraitDictionary(t);
    eggInstance = e;
  }
}
