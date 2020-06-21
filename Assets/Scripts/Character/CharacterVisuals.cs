using UnityEngine;

// This class exists only to map body parts to gameobjects to display visuals.
// Visuals are populated from scriptableobjects....... somehow
public class CharacterVisuals : MonoBehaviour
{
  public TraitSlotToCharacterCharacterBodyPartVisualDictionary traitSlotsToVisuals;

  public void SetCharacterVisuals(TraitSlotToTraitDictionary traits)
  {
    foreach (TraitSlot slot in traits.Keys)
    {
      if (!traitSlotsToVisuals.ContainsKey(slot))
      {
        Debug.LogError("Need to assign body part transforms to character visuals on character " + gameObject.name);
      }
      else
      {
        CharacterBodyPartVisual visual = traitSlotsToVisuals[slot];
        visual.SetSpritesFromTrait(traits[slot]);
        visual.gameObject.SetActive(true);
      }
    }
  }

  public void ClearCharacterVisuals()
  {

    foreach (TraitSlot slot in traitSlotsToVisuals.Keys)
    {
      CharacterBodyPartVisual visual = traitSlotsToVisuals[slot];
      visual.ClearSprites();
      visual.gameObject.SetActive(false);
    }
  }
}
