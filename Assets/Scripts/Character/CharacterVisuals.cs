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
        if (visual.visualRenderer1 != null)
        {
          visual.visualRenderer1.sprite = traits[slot].visual1;
        }
        if (visual.visualRenderer2 != null)
        {
          visual.visualRenderer2.sprite = traits[slot].visual2;
        }
      }
    }
  }
}
